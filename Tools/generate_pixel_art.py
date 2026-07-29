"""Dungeon Odyssey — higher quality pixel art (PPU 16)."""
from __future__ import annotations

from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter, ImageEnhance
import math
import random

ROOT = Path(__file__).resolve().parents[1] / "Assets" / "Resources" / "Art"
ROOT.mkdir(parents=True, exist_ok=True)
RNG = random.Random(42)


def save(img: Image.Image, name: str) -> None:
    # Nearest-neighbor friendly
    img = img.convert("RGBA")
    path = ROOT / f"{name}.png"
    img.save(path)
    print(f"wrote {path} ({img.size[0]}x{img.size[1]})")


def new(w: int, h: int) -> Image.Image:
    return Image.new("RGBA", (w, h), (0, 0, 0, 0))


def put(img: Image.Image, x: int, y: int, c) -> None:
    if 0 <= x < img.width and 0 <= y < img.height:
        img.putpixel((x, y), c)


def blend(a, b, t: float):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3)) + (255,)


def disk(img, cx, cy, rx, ry, color, outline=None):
    d = ImageDraw.Draw(img)
    d.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=color, outline=outline)


def shade_disk(img, cx, cy, rx, ry, base, light, dark):
    for y in range(int(cy - ry), int(cy + ry) + 1):
        for x in range(int(cx - rx), int(cx + rx) + 1):
            nx = (x - cx) / max(rx, 1)
            ny = (y - cy) / max(ry, 1)
            if nx * nx + ny * ny > 1:
                continue
            # light from top-left
            lum = 0.55 - 0.35 * nx - 0.45 * ny
            lum = max(0.0, min(1.0, lum))
            if lum > 0.55:
                c = blend(base, light, (lum - 0.55) / 0.45)
            else:
                c = blend(dark, base, lum / 0.55)
            # outline rim
            if nx * nx + ny * ny > 0.82:
                c = blend(c, (20, 20, 28), 0.55)
            put(img, x, y, c)


def rect(img, x0, y0, x1, y1, c):
    d = ImageDraw.Draw(img)
    d.rectangle([x0, y0, x1, y1], fill=c)


def outline_nonzero(img, color=(25, 22, 35, 255)):
    src = img.copy()
    out = img.copy()
    for y in range(img.height):
        for x in range(img.width):
            if src.getpixel((x, y))[3] == 0:
                # if neighbor opaque, draw outline
                for dx, dy in ((-1, 0), (1, 0), (0, -1), (0, 1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < img.width and 0 <= ny < img.height and src.getpixel((nx, ny))[3] > 0:
                        put(out, x, y, color)
                        break
    return out


# ---------- Characters ----------

def make_player(frame: str) -> Image.Image:
    img = new(48, 48)
    # soft shadow
    disk(img, 24, 42, 10, 3, (0, 0, 0, 75))

    # legs
    if frame == "idle":
        rect(img, 17, 30, 21, 39, (45, 58, 95, 255))
        rect(img, 26, 30, 30, 39, (45, 58, 95, 255))
        rect(img, 16, 38, 22, 41, (55, 40, 35, 255))
        rect(img, 25, 38, 31, 41, (55, 40, 35, 255))
        body_y = 0
        sword_x = 34
    elif frame == "walk_a":
        rect(img, 15, 30, 20, 38, (45, 58, 95, 255))
        rect(img, 27, 32, 32, 41, (45, 58, 95, 255))
        rect(img, 14, 37, 20, 40, (55, 40, 35, 255))
        rect(img, 28, 39, 34, 42, (55, 40, 35, 255))
        body_y = -1
        sword_x = 35
    else:  # walk_b
        rect(img, 18, 32, 23, 41, (45, 58, 95, 255))
        rect(img, 25, 30, 30, 38, (45, 58, 95, 255))
        rect(img, 17, 39, 23, 42, (55, 40, 35, 255))
        rect(img, 24, 37, 30, 40, (55, 40, 35, 255))
        body_y = 0
        sword_x = 33

    by = 18 + body_y
    # tunic with shading
    for y in range(by, by + 14):
        for x in range(15, 33):
            t = (x - 15) / 18
            edge = x in (15, 32) or y in (by, by + 13)
            base = (70, 145, 220)
            light = (130, 195, 245)
            dark = (40, 95, 160)
            c = blend(dark, light, 0.35 + 0.4 * (1 - abs(t - 0.35)))
            if edge:
                c = (30, 55, 95, 255)
            else:
                c = (*c[:3], 255) if isinstance(c, tuple) and len(c) == 3 else c
                if len(c) == 3:
                    c = (*c, 255)
            put(img, x, y, c if len(c) == 4 else (*c, 255))

    # belt + buckle
    rect(img, 15, by + 8, 32, by + 10, (195, 150, 55, 255))
    rect(img, 22, by + 7, 26, by + 11, (235, 205, 90, 255))

    # cape bit
    for y in range(by + 2, by + 12):
        put(img, 14, y, (50, 90, 160, 255))

    # head
    hx, hy = 24, 11 + body_y
    shade_disk(img, hx, hy, 7, 7, (235, 195, 155), (250, 220, 190), (190, 145, 110))
    # hair
    shade_disk(img, hx, hy - 3, 7, 5, (70, 45, 30), (110, 75, 45), (40, 25, 18))
    # bangs
    rect(img, 18, hy - 2, 30, hy, (60, 38, 25, 255))
    # eyes
    put(img, hx - 3, hy + 1, (35, 40, 55, 255))
    put(img, hx + 2, hy + 1, (35, 40, 55, 255))
    put(img, hx - 3, hy, (255, 255, 255, 200))
    put(img, hx + 2, hy, (255, 255, 255, 200))
    # blush
    put(img, hx - 5, hy + 3, (230, 150, 140, 120))
    put(img, hx + 4, hy + 3, (230, 150, 140, 120))

    # sword
    for y in range(8, 30):
        put(img, sword_x, y, (210, 220, 235, 255))
        put(img, sword_x + 1, y, (170, 180, 200, 255))
    rect(img, sword_x - 2, 28, sword_x + 3, 30, (160, 115, 50, 255))
    put(img, sword_x, 7, (245, 250, 255, 255))
    # sword shine
    put(img, sword_x, 12, (255, 255, 255, 220))
    put(img, sword_x, 18, (255, 255, 255, 160))

    return outline_nonzero(img, (22, 20, 32, 255))


def make_slime(frame: int) -> Image.Image:
    img = new(48, 48)
    disk(img, 24, 42, 12, 3, (0, 0, 0, 70))
    squash = frame  # 0/1/2
    cy = 26 + (1 if squash == 1 else 0)
    rx = 14 + (2 if squash == 1 else 0) - (1 if squash == 2 else 0)
    ry = 12 - (2 if squash == 1 else 0) + (1 if squash == 2 else 0)

    # body shaded
    for y in range(48):
        for x in range(48):
            nx = (x - 24) / rx
            ny = (y - cy) / ry
            if nx * nx + ny * ny > 1:
                continue
            # gelatin lighting
            lum = 0.5 - 0.3 * nx - 0.5 * ny
            lum = max(0, min(1, lum))
            base = (55, 195, 95)
            light = (160, 245, 170)
            dark = (25, 110, 55)
            if lum > 0.55:
                c = blend(base, light, (lum - 0.55) / 0.45)
            else:
                c = blend(dark, base, lum / 0.55)
            if nx * nx + ny * ny > 0.78:
                c = blend(c, (15, 50, 30), 0.5)
            # translucent top
            if ny < -0.2 and lum > 0.6:
                c = blend(c, (200, 255, 210), 0.25)
            put(img, x, y, (*c[:3], 255))

    # highlight blob
    disk(img, 18, cy - 5, 3, 2, (210, 255, 220, 180))
    # eyes
    ey = cy - 1 + (1 if squash == 1 else 0)
    rect(img, 17, ey, 20, ey + 4, (25, 45, 30, 255))
    rect(img, 27, ey, 30, ey + 4, (25, 45, 30, 255))
    put(img, 18, ey + 1, (255, 255, 255, 255))
    put(img, 28, ey + 1, (255, 255, 255, 255))
    # cheeks
    put(img, 14, ey + 3, (255, 140, 140, 100))
    put(img, 32, ey + 3, (255, 140, 140, 100))
    # mouth
    if squash != 2:
        rect(img, 21, ey + 5, 26, ey + 6, (30, 90, 45, 255))
    else:
        rect(img, 20, ey + 4, 27, ey + 7, (30, 90, 45, 255))

    return outline_nonzero(img)


def make_npc() -> Image.Image:
    img = new(48, 48)
    disk(img, 24, 42, 10, 3, (0, 0, 0, 70))
    # legs
    rect(img, 18, 32, 22, 40, (80, 55, 40, 255))
    rect(img, 25, 32, 29, 40, (80, 55, 40, 255))
    # robe
    for y in range(18, 36):
        for x in range(14, 34):
            width = 10 + (y - 18) // 3
            if abs(x - 24) > width:
                continue
            t = (x - 14) / 20
            c = blend((160, 110, 30), (240, 200, 70), 0.35 + 0.4 * (1 - abs(t - 0.4)))
            if abs(x - 24) >= width - 1:
                c = (90, 60, 20)
            put(img, x, y, (*c[:3], 255))
    # hood
    shade_disk(img, 24, 13, 9, 8, (190, 140, 45), (230, 190, 80), (120, 80, 25))
    shade_disk(img, 24, 15, 6, 6, (235, 200, 160), (250, 225, 195), (190, 150, 120))
    put(img, 21, 15, (40, 30, 25, 255))
    put(img, 26, 15, (40, 30, 25, 255))
    # smile
    put(img, 22, 18, (180, 100, 90, 200))
    put(img, 23, 19, (180, 100, 90, 200))
    put(img, 24, 19, (180, 100, 90, 200))
    put(img, 25, 18, (180, 100, 90, 200))
    # staff
    for y in range(6, 42):
        put(img, 37, y, (120, 80, 40, 255))
        put(img, 38, y, (90, 60, 30, 255))
    disk(img, 38, 7, 5, 5, (60, 170, 210, 255), (30, 90, 130, 255))
    disk(img, 38, 7, 2, 2, (200, 245, 255, 255))
    # glow dots
    put(img, 35, 5, (180, 240, 255, 180))
    put(img, 41, 9, (180, 240, 255, 180))
    return outline_nonzero(img)


def make_door() -> Image.Image:
    img = new(48, 64)
    d = ImageDraw.Draw(img)
    # stone arch
    d.rectangle([4, 12, 43, 63], fill=(75, 72, 90, 255))
    d.ellipse([4, 0, 43, 40], fill=(75, 72, 90, 255))
    d.ellipse([10, 8, 37, 42], fill=(35, 22, 55, 255))
    d.rectangle([10, 24, 37, 61], fill=(35, 22, 55, 255))
    # wood
    for x in range(12, 24):
        for y in range(18, 60):
            c = blend((95, 55, 130), (70, 40, 100), (y % 7) / 7)
            put(img, x, y, (*c[:3], 255))
    for x in range(24, 36):
        for y in range(18, 60):
            c = blend((85, 48, 120), (60, 35, 90), (y % 7) / 7)
            put(img, x, y, (*c[:3], 255))
    # metal bands
    rect(img, 12, 28, 35, 30, (160, 150, 170, 255))
    rect(img, 12, 46, 35, 48, (160, 150, 170, 255))
    # glowing rune
    for y in range(30, 48):
        put(img, 23, y, (120, 230, 255, 230))
        put(img, 24, y, (180, 245, 255, 200))
    # knob
    disk(img, 31, 40, 3, 3, (230, 195, 70, 255), (140, 110, 40, 255))
    # moss
    for x, y in [(6, 50), (7, 52), (40, 48), (41, 51), (8, 20)]:
        put(img, x, y, (60, 120, 55, 200))
    return outline_nonzero(img)


def make_portal(frame: int = 0) -> Image.Image:
    img = new(48, 48)
    cx, cy = 24, 24
    for y in range(48):
        for x in range(48):
            dx, dy = x - cx, y - cy
            dist = math.sqrt(dx * dx + dy * dy)
            ang = math.atan2(dy, dx) + frame * 0.4
            if dist > 20:
                continue
            ring = abs(dist - 14)
            swirl = 0.5 + 0.5 * math.sin(ang * 3 + dist * 0.4)
            if dist < 6:
                c = blend((255, 255, 255), (180, 250, 255), dist / 6)
                a = 240
            elif dist < 12:
                c = blend((100, 220, 255), (40, 120, 200), (dist - 6) / 6)
                a = 220
            else:
                c = blend((40, 100, 180), (20, 40, 80), (dist - 12) / 8)
                a = int(200 * (1 - (dist - 12) / 8))
            if ring < 1.2:
                c = blend(c, (200, 255, 255), 0.6)
            c = blend(c, (150, 230, 255), swirl * 0.15)
            put(img, x, y, (*c[:3], a))
    return img


# ---------- Tiles ----------

def make_tile(kind: str) -> Image.Image:
    img = new(16, 16)
    if kind == "grass":
        base = (58, 130, 62)
        for y in range(16):
            for x in range(16):
                n = RNG.random()
                c = blend(base, (78, 155, 70), n * 0.35)
                if n > 0.92:
                    c = (95, 175, 80)
                if n < 0.08:
                    c = (45, 105, 50)
                put(img, x, y, (*c[:3], 255))
        # grass blades
        for x, y in [(3, 4), (9, 2), (12, 8), (5, 11), (14, 13), (1, 9)]:
            put(img, x, y, (110, 190, 90, 255))
            put(img, x, y - 1, (90, 170, 75, 220))
    elif kind == "path":
        base = (168, 138, 92)
        for y in range(16):
            for x in range(16):
                n = RNG.random()
                c = blend(base, (145, 115, 75), n * 0.4)
                if n > 0.9:
                    c = (190, 160, 110)
                put(img, x, y, (*c[:3], 255))
        # pebbles
        for x, y in [(4, 5), (11, 3), (7, 10), (13, 12)]:
            put(img, x, y, (120, 95, 65, 255))
    elif kind == "stone":
        for y in range(16):
            for x in range(16):
                cell = (x // 8) + (y // 8) * 2
                tones = [(120, 118, 130), (105, 104, 118), (130, 128, 140), (95, 94, 108)]
                c = tones[cell % 4]
                if x % 8 == 0 or y % 8 == 0:
                    c = blend(c, (70, 70, 82), 0.5)
                put(img, x, y, (*c[:3], 255))
    else:  # dungeon
        for y in range(16):
            for x in range(16):
                gx, gy = x // 8, y // 8
                base = (48 + gx * 4, 44 + gy * 3, 62)
                c = blend(base, (35, 32, 48), ((x + y) % 5) / 8)
                if x % 8 == 0 or y % 8 == 0:
                    c = (28, 26, 38)
                if RNG.random() > 0.97:
                    c = (70, 65, 90)
                put(img, x, y, (*c[:3], 255))
    return img


def make_house() -> Image.Image:
    img = new(64, 64)
    d = ImageDraw.Draw(img)
    # shadow
    d.ellipse([10, 54, 54, 62], fill=(0, 0, 0, 60))
    # wall
    for y in range(26, 56):
        for x in range(10, 54):
            c = blend((175, 125, 80), (210, 160, 105), (x - 10) / 50 * 0.4 + 0.3)
            if x in (10, 53) or y == 55:
                c = (90, 60, 40)
            put(img, x, y, (*c[:3], 255))
    # timber
    rect(img, 10, 34, 53, 36, (110, 75, 45, 255))
    # roof
    for y in range(8, 30):
        span = int(22 * (y - 8) / 22)
        for x in range(32 - span - 2, 32 + span + 2):
            t = (y - 8) / 22
            c = blend((200, 70, 55), (140, 45, 40), t)
            if x in (32 - span - 2, 32 + span + 1):
                c = (90, 35, 30)
            put(img, x, y, (*c[:3], 255))
    # chimney
    rect(img, 42, 10, 48, 24, (100, 95, 105, 255))
    # door
    rect(img, 27, 38, 37, 55, (95, 60, 40, 255))
    rect(img, 28, 39, 36, 54, (120, 75, 45, 255))
    put(img, 34, 46, (220, 185, 70, 255))
    # windows
    for wx in (14, 42):
        rect(img, wx, 40, wx + 8, 48, (40, 55, 70, 255))
        rect(img, wx + 1, 41, wx + 7, 47, (140, 210, 235, 255))
        put(img, wx + 4, 41, (255, 255, 255, 180))
    return outline_nonzero(img)


def make_tree() -> Image.Image:
    img = new(48, 64)
    disk(img, 24, 58, 10, 3, (0, 0, 0, 60))
    # trunk
    for y in range(36, 58):
        for x in range(20, 28):
            c = blend((100, 70, 40), (70, 45, 25), (x - 20) / 8)
            put(img, x, y, (*c[:3], 255))
    # foliage layers
    for i, (cy, rx, ry, base) in enumerate([
        (28, 18, 14, (35, 115, 50)),
        (22, 16, 13, (50, 145, 65)),
        (16, 12, 10, (75, 175, 85)),
    ]):
        shade_disk(img, 24, cy, rx, ry, base, blend(base, (180, 230, 140), 0.5), blend(base, (20, 60, 30), 0.5))
    return outline_nonzero(img)


def make_slash(frame: int) -> Image.Image:
    img = new(48, 48)
    d = ImageDraw.Draw(img)
    start = 200 - frame * 15
    end = 340 - frame * 10
    d.arc([4, 4, 44, 44], start, end, fill=(255, 255, 230, 255), width=4)
    d.arc([8, 8, 40, 40], start + 5, end - 5, fill=(160, 230, 255, 200), width=2)
    # spark
    put(img, 36 - frame * 2, 16, (255, 255, 255, 255))
    put(img, 34 - frame * 2, 18, (255, 240, 180, 220))
    return img


def make_title_banner() -> Image.Image:
    img = new(384, 96)
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([8, 12, 375, 84], radius=16, fill=(16, 20, 36, 235), outline=(220, 175, 70, 255), width=4)
    d.rounded_rectangle([14, 18, 369, 78], radius=12, fill=(24, 30, 50, 180), outline=(90, 120, 170, 120), width=2)
    # gems
    for cx in (36, 348):
        d.ellipse([cx - 10, 38, cx + 10, 58], fill=(70, 180, 240, 255), outline=(230, 250, 255, 255))
        put(img, cx - 3, 44, (255, 255, 255, 255))
    # chain dots
    for x in range(60, 330, 14):
        put(img, x, 22, (200, 160, 70, 200))
        put(img, x, 74, (200, 160, 70, 200))
    return img


def make_shadow() -> Image.Image:
    img = new(32, 16)
    disk(img, 16, 8, 12, 5, (0, 0, 0, 90))
    return img


def make_torch() -> Image.Image:
    img = new(16, 32)
    rect(img, 6, 14, 9, 30, (90, 60, 35, 255))
    disk(img, 8, 10, 5, 6, (255, 160, 40, 255))
    disk(img, 8, 8, 3, 4, (255, 220, 80, 255))
    put(img, 8, 5, (255, 255, 200, 255))
    return outline_nonzero(img)


def main() -> None:
    save(make_player("idle"), "player_idle")
    save(make_player("walk_a"), "player_walk")
    save(make_player("walk_b"), "player_walk_b")
    save(make_slime(0), "slime_a")
    save(make_slime(1), "slime_b")
    save(make_slime(2), "slime_c")
    save(make_npc(), "npc_guide")
    save(make_door(), "dungeon_door")
    save(make_portal(0), "portal")
    save(make_portal(1), "portal_b")
    save(make_tile("grass"), "tile_grass")
    save(make_tile("path"), "tile_path")
    save(make_tile("stone"), "tile_stone")
    save(make_tile("dungeon"), "tile_dungeon")
    # tile variants
    save(make_tile("grass"), "tile_grass_b")
    save(make_tile("dungeon"), "tile_dungeon_b")
    save(make_house(), "house")
    save(make_tree(), "tree")
    save(make_slash(0), "slash")
    save(make_slash(1), "slash_b")
    save(make_title_banner(), "title_banner")
    save(make_shadow(), "shadow")
    save(make_torch(), "torch")
    print("done")


if __name__ == "__main__":
    main()
