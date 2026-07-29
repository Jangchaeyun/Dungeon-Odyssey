"""Town + dungeon environment art upgrade."""
from __future__ import annotations

from pathlib import Path
from PIL import Image, ImageDraw
import random
import math

ROOT = Path(__file__).resolve().parents[1] / "Assets" / "Resources" / "Art"
ROOT.mkdir(parents=True, exist_ok=True)
RNG = random.Random(7)


def save(img: Image.Image, name: str) -> None:
    path = ROOT / f"{name}.png"
    img.save(path)
    print(f"wrote {path}")


def new(w, h):
    return Image.new("RGBA", (w, h), (0, 0, 0, 0))


def put(img, x, y, c):
    if not (0 <= x < img.width and 0 <= y < img.height):
        return
    if isinstance(c, tuple):
        if len(c) > 4:
            c = c[:4]
        elif len(c) == 3:
            c = (*c, 255)
        elif len(c) == 2:
            c = (c[0], c[0], c[0], c[1])
    img.putpixel((x, y), c)


def blend(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3)) + (255,)


def outline(img, color=(30, 26, 38, 255)):
    src = img.copy()
    out = img.copy()
    for y in range(img.height):
        for x in range(img.width):
            if src.getpixel((x, y))[3] != 0:
                continue
            for dx, dy in ((-1, 0), (1, 0), (0, -1), (0, 1)):
                nx, ny = x + dx, y + dy
                if 0 <= nx < img.width and 0 <= ny < img.height and src.getpixel((nx, ny))[3] > 0:
                    put(out, x, y, color)
                    break
    return out


def rect(img, x0, y0, x1, y1, c):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            put(img, x, y, c)


# ---- Tiles 32x32 for richer detail (still PPU 16 => 2 world units; we'll use as 1-tile visually via scale or keep 16)
# Keep 16x16 but much richer, plus 32x32 props.

def tile_grass(variant=0):
    img = new(16, 16)
    base = (56, 128, 58) if variant == 0 else (62, 136, 64)
    for y in range(16):
        for x in range(16):
            n = RNG.random()
            c = blend(base, (78, 158, 72), n * 0.4)
            if n > 0.93:
                c = (105, 180, 90, 255)
            if n < 0.07:
                c = (42, 100, 48, 255)
            # subtle dirt patches
            if variant == 1 and 5 <= x <= 9 and 6 <= y <= 10 and n > 0.5:
                c = blend(c[:3], (110, 95, 55), 0.35)
            put(img, x, y, c if len(c) == 4 else (*c, 255))
    # blades
    for x, y in [(2, 4), (8, 2), (13, 6), (5, 10), (11, 12), (1, 13), (14, 9), (7, 7)]:
        put(img, x, y, (120, 195, 95, 255))
        if y > 0:
            put(img, x, y - 1, (95, 170, 80, 200))
    # tiny flowers
    if variant == 0:
        put(img, 4, 8, (240, 220, 90, 255))
        put(img, 12, 3, (255, 160, 180, 255))
    return img


def tile_path():
    img = new(16, 16)
    for y in range(16):
        for x in range(16):
            n = RNG.random()
            c = blend((165, 135, 90), (140, 112, 72), n * 0.45)
            if n > 0.9:
                c = (190, 160, 110, 255)
            put(img, x, y, c if len(c) == 4 else (*c, 255))
    # cobble cracks
    for x, y in [(3, 4), (10, 2), (7, 9), (13, 12), (1, 11), (8, 6), (5, 14)]:
        put(img, x, y, (115, 90, 60, 255))
    for x in range(16):
        if x % 4 == 0:
            put(img, x, 8, (120, 95, 65, 180))
    return img


def tile_plaza():
    img = new(16, 16)
    for y in range(16):
        for x in range(16):
            brick_x = x // 8
            brick_y = y // 4
            tones = [(150, 145, 155), (135, 132, 142), (160, 155, 165), (125, 122, 132)]
            c = tones[(brick_x + brick_y) % 4]
            if x % 8 == 0 or y % 4 == 0:
                c = (90, 88, 98)
            # wear
            if RNG.random() > 0.96:
                c = blend(c[:3], (100, 90, 70), 0.4)
            put(img, x, y, c if len(c) == 4 else (*c, 255))
    return img


def tile_dungeon(variant=0):
    img = new(16, 16)
    for y in range(16):
        for x in range(16):
            gx, gy = x // 8, y // 8
            base = (46 + gx * 5 + variant * 3, 42 + gy * 3, 60 + variant * 2)
            c = blend(base, (30, 28, 42), ((x + y + variant) % 6) / 10)
            if x % 8 == 0 or y % 8 == 0:
                c = (22, 20, 32)
            if RNG.random() > 0.97:
                c = (70, 65, 95)  # crystal fleck
            if variant == 1 and RNG.random() > 0.95:
                c = (55, 80, 55, 255)  # moss
            put(img, x, y, c if len(c) == 4 else (*c, 255))
    return img


def tile_wall_top():
    """Dungeon wall face — used as northern wall strip."""
    img = new(16, 24)
    for y in range(24):
        for x in range(16):
            # top lip
            if y < 4:
                c = blend((70, 68, 85), (95, 92, 110), x / 16)
            elif y < 18:
                brick = (x // 4) + (y // 5) * 3
                tones = [(58, 56, 72), (50, 48, 64), (64, 62, 78)]
                c = tones[brick % 3]
                if x % 4 == 0 or y % 5 == 0:
                    c = (35, 33, 48)
            else:
                c = (28, 26, 38)  # shadow base
            put(img, x, y, c)
    # torch sconce slot hint
    rect(img, 6, 8, 9, 14, (40, 35, 50, 255))
    return img


def tile_wall_side():
    img = new(8, 16)
    for y in range(16):
        for x in range(8):
            c = blend((48, 46, 62), (35, 33, 48), x / 8)
            if y % 5 == 0:
                c = (28, 26, 40, 255)
            put(img, x, y, c)
    return img


def fence():
    img = new(16, 16)
    # posts
    rect(img, 2, 4, 4, 14, (120, 85, 50, 255))
    rect(img, 12, 4, 14, 14, (120, 85, 50, 255))
    rect(img, 2, 6, 14, 8, (140, 100, 55, 255))
    rect(img, 2, 10, 14, 12, (140, 100, 55, 255))
    return outline(img)


def well():
    img = new(32, 32)
    # shadow
    for y in range(24, 30):
        for x in range(6, 26):
            if (x - 16) ** 2 / 100 + (y - 26) ** 2 / 16 <= 1:
                put(img, x, y, (0, 0, 0, 60))
    # stone ring
    for y in range(32):
        for x in range(32):
            dx, dy = x - 16, y - 16
            d = math.sqrt(dx * dx + dy * dy * 1.1)
            if 9 < d < 13:
                c = blend((130, 125, 135), (90, 88, 100), (dx + 16) / 32)
                put(img, x, y, (*c, 255))
            elif d <= 9:
                put(img, x, y, (25, 40, 70, 255))
    # water shine
    put(img, 13, 14, (120, 180, 220, 200))
    put(img, 15, 15, (160, 210, 240, 180))
    # roof posts
    rect(img, 8, 4, 10, 14, (100, 70, 40, 255))
    rect(img, 21, 4, 23, 14, (100, 70, 40, 255))
    rect(img, 7, 3, 24, 6, (160, 60, 50, 255))
    return outline(img)


def crate():
    img = new(16, 16)
    rect(img, 2, 4, 13, 14, (150, 105, 60, 255))
    rect(img, 3, 5, 12, 13, (175, 125, 70, 255))
    rect(img, 2, 8, 13, 9, (120, 80, 45, 255))
    rect(img, 7, 4, 8, 14, (120, 80, 45, 255))
    return outline(img)


def barrel():
    img = new(16, 18)
    for y in range(3, 16):
        for x in range(3, 13):
            nx = (x - 8) / 5
            if abs(nx) > 1:
                continue
            c = blend((120, 80, 45), (170, 120, 65), 0.5 - abs(nx) * 0.3)
            put(img, x, y, (*c, 255))
    rect(img, 3, 6, 12, 7, (80, 75, 85, 255))
    rect(img, 3, 12, 12, 13, (80, 75, 85, 255))
    return outline(img)


def chest():
    img = new(20, 16)
    rect(img, 2, 6, 17, 14, (150, 100, 45, 255))
    rect(img, 2, 3, 17, 8, (180, 130, 55, 255))
    rect(img, 2, 7, 17, 8, (200, 160, 60, 255))
    rect(img, 9, 8, 11, 11, (220, 190, 70, 255))
    return outline(img)


def flower_pot():
    img = new(16, 16)
    rect(img, 5, 9, 10, 14, (160, 90, 60, 255))
    # leaves
    put(img, 6, 6, (50, 160, 70, 255))
    put(img, 8, 4, (60, 175, 80, 255))
    put(img, 9, 7, (45, 150, 65, 255))
    put(img, 7, 5, (230, 80, 100, 255))
    return outline(img)


def dungeon_pillar():
    img = new(16, 32)
    rect(img, 4, 4, 11, 28, (70, 68, 85, 255))
    rect(img, 5, 5, 10, 27, (90, 88, 105, 255))
    rect(img, 3, 3, 12, 5, (110, 108, 125, 255))
    rect(img, 3, 27, 12, 29, (55, 53, 70, 255))
    # cracks
    put(img, 7, 12, (50, 48, 62, 255))
    put(img, 8, 13, (50, 48, 62, 255))
    put(img, 8, 18, (50, 48, 62, 255))
    return outline(img)


def dungeon_bones():
    img = new(16, 12)
    rect(img, 2, 6, 12, 8, (230, 220, 200, 255))
    rect(img, 11, 4, 14, 9, (230, 220, 200, 255))
    put(img, 3, 5, (200, 190, 170, 255))
    return outline(img)


def rug():
    img = new(32, 20)
    for y in range(20):
        for x in range(32):
            if x in (0, 31) or y in (0, 19):
                put(img, x, y, (120, 40, 40, 255))
            else:
                c = (160, 50, 50) if (x // 4 + y // 3) % 2 == 0 else (140, 40, 45)
                put(img, x, y, (*c, 255))
    return img


def house_big():
    img = new(80, 80)
    # shadow
    for y in range(68, 76):
        for x in range(8, 72):
            put(img, x, y, (0, 0, 0, 50))
    # wall
    for y in range(32, 70):
        for x in range(10, 70):
            c = blend((185, 140, 95), (210, 165, 115), (x - 10) / 60 * 0.35)
            if x in (10, 69) or y == 69:
                c = (100, 70, 45)
            # timber beams
            if y in (45, 58) or x in (30, 50):
                c = blend(c, (110, 75, 45), 0.55)
            put(img, x, y, (*c, 255))
    # roof
    for y in range(10, 36):
        span = int(28 * (y - 10) / 26)
        for x in range(40 - span - 3, 40 + span + 3):
            t = (y - 10) / 26
            c = blend((200, 75, 55), (140, 50, 40), t)
            if x in (40 - span - 3, 40 + span + 2):
                c = (90, 40, 30)
            put(img, x, y, (*c, 255))
    # chimney + smoke hint
    rect(img, 54, 12, 62, 28, (105, 100, 110, 255))
    put(img, 57, 8, (180, 180, 190, 120))
    put(img, 59, 5, (180, 180, 190, 80))
    # door
    rect(img, 34, 48, 46, 69, (95, 60, 35, 255))
    rect(img, 35, 49, 45, 68, (125, 80, 45, 255))
    put(img, 43, 58, (220, 185, 70, 255))
    # windows
    for wx in (16, 54):
        rect(img, wx, 48, wx + 10, 58, (40, 60, 80, 255))
        rect(img, wx + 1, 49, wx + 9, 57, (150, 215, 240, 255))
        put(img, wx + 3, 50, (255, 255, 255, 180))
        rect(img, wx + 5, 49, wx + 5, 57, (40, 60, 80, 200))
        rect(img, wx + 1, 53, wx + 9, 53, (40, 60, 80, 200))
    # flower boxes
    rect(img, 15, 58, 27, 61, (80, 140, 60, 255))
    rect(img, 53, 58, 65, 61, (80, 140, 60, 255))
    return outline(img)


def tree_big():
    img = new(64, 80)
    for y in range(70, 78):
        for x in range(20, 44):
            if (x - 32) ** 2 / 120 + (y - 73) ** 2 / 20 <= 1:
                put(img, x, y, (0, 0, 0, 55))
    # trunk
    for y in range(48, 74):
        for x in range(27, 37):
            c = blend((110, 75, 42), (70, 48, 28), (x - 27) / 10)
            put(img, x, y, (*c, 255))
    # canopy layers
    layers = [(42, 26, 22, (30, 110, 48)), (34, 24, 20, (45, 140, 60)), (26, 20, 16, (70, 170, 80)), (20, 14, 12, (100, 195, 100))]
    for cy, rx, ry, base in layers:
        light = blend(base, (180, 230, 140), 0.45)
        dark = blend(base, (15, 55, 25), 0.45)
        for y in range(cy - ry, cy + ry + 1):
            for x in range(32 - rx, 32 + rx + 1):
                nx = (x - 32) / rx
                ny = (y - cy) / ry
                if nx * nx + ny * ny > 1:
                    continue
                lum = 0.5 - 0.25 * nx - 0.4 * ny
                lum = max(0, min(1, lum))
                c = blend(dark, light, lum) if lum < 0.55 else blend(base, light, (lum - 0.55) / 0.45)
                put(img, x, y, (*c, 255))
    return outline(img)


def grass_tuft():
    img = new(12, 12)
    for i, (x, h) in enumerate([(2, 7), (5, 9), (8, 6)]):
        for y in range(11 - h, 11):
            put(img, x, y, (70 + i * 10, 160, 70, 255))
    return img


def carpet_dungeon():
    img = new(48, 32)
    for y in range(32):
        for x in range(48):
            if x < 2 or x > 45 or y < 2 or y > 29:
                put(img, x, y, (60, 40, 80, 255))
            else:
                c = (90, 45, 110) if ((x + y) // 4) % 2 == 0 else (75, 35, 95)
                put(img, x, y, (*c, 230))
    return img


def main():
    save(tile_grass(0), "tile_grass")
    save(tile_grass(1), "tile_grass_b")
    save(tile_path(), "tile_path")
    save(tile_plaza(), "tile_stone")
    save(tile_dungeon(0), "tile_dungeon")
    save(tile_dungeon(1), "tile_dungeon_b")
    save(tile_wall_top(), "wall_dungeon")
    save(tile_wall_side(), "wall_side")
    save(fence(), "prop_fence")
    save(well(), "prop_well")
    save(crate(), "prop_crate")
    save(barrel(), "prop_barrel")
    save(chest(), "prop_chest")
    save(flower_pot(), "prop_flower")
    save(dungeon_pillar(), "prop_pillar")
    save(dungeon_bones(), "prop_bones")
    save(rug(), "prop_rug")
    save(carpet_dungeon(), "prop_carpet")
    save(house_big(), "house")
    save(tree_big(), "tree")
    save(grass_tuft(), "prop_grass")
    print("done")


if __name__ == "__main__":
    main()
