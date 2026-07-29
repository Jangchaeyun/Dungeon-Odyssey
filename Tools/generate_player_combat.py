"""More human player + realistic attack frames / slash FX."""
from __future__ import annotations

from pathlib import Path
from PIL import Image, ImageDraw
import math

ROOT = Path(__file__).resolve().parents[1] / "Assets" / "Resources" / "Art"
ROOT.mkdir(parents=True, exist_ok=True)


def save(img: Image.Image, name: str) -> None:
    path = ROOT / f"{name}.png"
    img.save(path)
    print(f"wrote {path}")


def new(w=64, h=64) -> Image.Image:
    return Image.new("RGBA", (w, h), (0, 0, 0, 0))


def put(img, x, y, c):
    if 0 <= x < img.width and 0 <= y < img.height:
        img.putpixel((x, y), c)


def blend(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3)) + (a[3] if len(a) > 3 else 255,)


def outline(img, color=(28, 24, 36, 255)):
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


def disk(img, cx, cy, rx, ry, base, light, dark):
    for y in range(int(cy - ry - 1), int(cy + ry + 2)):
        for x in range(int(cx - rx - 1), int(cx + rx + 2)):
            nx = (x - cx) / max(rx, 1)
            ny = (y - cy) / max(ry, 1)
            if nx * nx + ny * ny > 1:
                continue
            lum = 0.55 - 0.3 * nx - 0.45 * ny
            lum = max(0.0, min(1.0, lum))
            if lum > 0.55:
                c = blend(base, light, (lum - 0.55) / 0.45)
            else:
                c = blend(dark, base, lum / 0.55)
            if nx * nx + ny * ny > 0.8:
                c = blend(c, (20, 18, 28, 255), 0.45)
            put(img, x, y, c)


def rect(img, x0, y0, x1, y1, c):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            put(img, x, y, c)


def draw_sword(img, x0, y0, x1, y1, tip_boost=True):
    """Bresenham-ish thick sword line with metal shading."""
    steps = max(abs(x1 - x0), abs(y1 - y0), 1)
    for i in range(steps + 1):
        t = i / steps
        x = int(x0 + (x1 - x0) * t)
        y = int(y0 + (y1 - y0) * t)
        # blade thickness
        for ox, oy in ((0, 0), (1, 0), (0, 1), (-1, 0)):
            put(img, x + ox, y + oy, (205, 215, 230, 255))
        put(img, x, y, (235, 240, 250, 255))
        if i % 4 == 0:
            put(img, x - 1, y, (255, 255, 255, 220))
    # tip
    put(img, x1, y1, (255, 255, 255, 255))
    # guard / grip near base
    gx = int(x0 + (x1 - x0) * 0.18)
    gy = int(y0 + (y1 - y0) * 0.18)
    rect(img, gx - 2, gy - 1, gx + 2, gy + 1, (170, 130, 55, 255))
    rect(img, int(x0) - 1, int(y0) - 1, int(x0) + 1, int(y0) + 3, (90, 60, 40, 255))


def draw_human(img, pose: str):
    """
    pose: idle, walk_a, walk_b, atk_wind, atk_slash, atk_follow
    Canvas 64x64, feet near y=56, head ~y=16
    """
    # shadow
    for y in range(54, 60):
        for x in range(18, 46):
            dx = (x - 32) / 12
            dy = (y - 56) / 3
            if dx * dx + dy * dy <= 1:
                put(img, x, y, (0, 0, 0, 70))

    # pose offsets
    leg = {
        "idle": ((24, 42, 28, 54), (35, 42, 39, 54)),
        "walk_a": ((22, 42, 27, 53), (36, 44, 41, 56)),
        "walk_b": ((24, 44, 29, 56), (34, 42, 39, 53)),
        "atk_wind": ((25, 42, 29, 54), (34, 42, 38, 54)),
        "atk_slash": ((23, 43, 28, 54), (35, 42, 40, 54)),
        "atk_follow": ((24, 42, 29, 54), (34, 43, 39, 54)),
    }[pose]

    body_shift = {"idle": 0, "walk_a": -1, "walk_b": 0, "atk_wind": -1, "atk_slash": 1, "atk_follow": 0}[pose]
    bx = 32 + body_shift

    # boots + legs
    for (x0, y0, x1, y1) in leg:
        rect(img, x0, y0, x1, y1 - 3, (55, 70, 110, 255))
        rect(img, x0 - 1, y1 - 3, x1 + 1, y1, (60, 45, 38, 255))
        # highlight
        put(img, x0 + 1, y0 + 2, (80, 100, 150, 255))

    # hips / belt area
    rect(img, bx - 8, 38, bx + 7, 43, (45, 60, 100, 255))

    # torso armor
    for y in range(24, 40):
        for x in range(bx - 9, bx + 9):
            t = (x - (bx - 9)) / 18
            edge = x in (bx - 9, bx + 8) or y in (24, 39)
            c = blend((55, 115, 185), (110, 175, 235), 0.3 + 0.5 * (1 - abs(t - 0.4)))
            if edge:
                c = (35, 70, 120, 255)
            # chest plate highlight
            if 27 <= y <= 33 and bx - 4 <= x <= bx + 2:
                c = blend(c, (160, 210, 250, 255), 0.25)
            put(img, x, y, c)

    # belt + buckle
    rect(img, bx - 9, 36, bx + 8, 38, (190, 145, 55, 255))
    rect(img, bx - 2, 35, bx + 2, 39, (230, 200, 90, 255))

    # shoulder pads
    rect(img, bx - 11, 25, bx - 7, 30, (70, 130, 200, 255))
    rect(img, bx + 6, 25, bx + 10, 30, (70, 130, 200, 255))

    # neck
    rect(img, bx - 2, 20, bx + 1, 24, (220, 175, 140, 255))

    # head
    hx, hy = bx - 1, 14
    disk(img, hx, hy, 8, 8, (235, 190, 150, 255), (250, 220, 190, 255), (185, 140, 105, 255))

    # hair
    disk(img, hx, hy - 3, 8, 6, (75, 48, 32, 255), (120, 80, 50, 255), (45, 28, 18, 255))
    rect(img, hx - 7, hy - 2, hx + 6, hy + 1, (70, 45, 30, 255))
    # sideburns
    rect(img, hx - 8, hy, hx - 7, hy + 4, (70, 45, 30, 255))
    rect(img, hx + 6, hy, hx + 7, hy + 4, (70, 45, 30, 255))

    # face details
    put(img, hx - 3, hy + 1, (40, 45, 60, 255))  # eyes
    put(img, hx + 2, hy + 1, (40, 45, 60, 255))
    put(img, hx - 3, hy, (255, 255, 255, 220))
    put(img, hx + 2, hy, (255, 255, 255, 220))
    # brows
    put(img, hx - 4, hy - 1, (60, 40, 30, 255))
    put(img, hx + 3, hy - 1, (60, 40, 30, 255))
    # nose hint
    put(img, hx, hy + 2, (210, 160, 125, 255))
    # mouth
    put(img, hx - 1, hy + 4, (170, 100, 95, 255))
    put(img, hx, hy + 4, (170, 100, 95, 255))
    # blush
    put(img, hx - 5, hy + 3, (230, 140, 130, 100))
    put(img, hx + 4, hy + 3, (230, 140, 130, 100))

    # left arm (back) - depends on pose
    if pose in ("idle", "walk_a", "walk_b"):
        # hanging left arm
        rect(img, bx - 12, 28, bx - 9, 40, (220, 175, 140, 255))
        rect(img, bx - 12, 28, bx - 9, 34, (60, 120, 190, 255))  # sleeve
        # right arm holding sword down-right
        rect(img, bx + 8, 28, bx + 11, 36, (60, 120, 190, 255))
        rect(img, bx + 9, 35, bx + 12, 40, (220, 175, 140, 255))
        draw_sword(img, bx + 11, 40, bx + 22, 22)
    elif pose == "atk_wind":
        # wind up: sword raised behind
        rect(img, bx - 11, 26, bx - 8, 34, (60, 120, 190, 255))
        rect(img, bx + 7, 22, bx + 11, 30, (60, 120, 190, 255))
        rect(img, bx + 8, 18, bx + 12, 24, (220, 175, 140, 255))
        draw_sword(img, bx + 10, 20, bx + 6, 4)
        # determined brow
        put(img, hx - 4, hy - 1, (50, 30, 25, 255))
        put(img, hx + 3, hy - 1, (50, 30, 25, 255))
    elif pose == "atk_slash":
        # mid slash: body lean, sword sweeping across
        rect(img, bx - 10, 27, bx - 7, 35, (60, 120, 190, 255))
        rect(img, bx + 6, 26, bx + 14, 30, (60, 120, 190, 255))
        rect(img, bx + 12, 27, bx + 16, 31, (220, 175, 140, 255))
        draw_sword(img, bx + 14, 28, bx + 28, 20)
        # grit teeth
        put(img, hx - 1, hy + 4, (240, 220, 200, 255))
        put(img, hx, hy + 4, (240, 220, 200, 255))
    else:  # atk_follow
        rect(img, bx - 11, 28, bx - 8, 38, (60, 120, 190, 255))
        rect(img, bx + 7, 30, bx + 12, 36, (60, 120, 190, 255))
        rect(img, bx + 11, 34, bx + 14, 38, (220, 175, 140, 255))
        draw_sword(img, bx + 13, 36, bx + 26, 42)

    return outline(img)


def make_slash_arc(frame: int) -> Image.Image:
    img = new(80, 80)
    cx, cy = 40, 40
    # frame 0 wind glow, 1 main arc, 2 fade sparks
    if frame == 0:
        for a in range(-40, 10):
            rad = math.radians(a)
            for r in range(18, 34):
                x = int(cx + math.cos(rad) * r)
                y = int(cy + math.sin(rad) * r)
                put(img, x, y, (180, 220, 255, 90))
    elif frame == 1:
        for a in range(-50, 55):
            rad = math.radians(a)
            for r in range(16, 36):
                x = int(cx + math.cos(rad) * r)
                y = int(cy + math.sin(rad) * r)
                edge = r > 32 or r < 18
                c = (255, 255, 240, 255) if not edge else (140, 210, 255, 220)
                put(img, x, y, c)
                if a % 7 == 0 and r == 30:
                    put(img, x, y - 1, (255, 255, 255, 255))
        # core streak
        for i in range(20):
            put(img, 48 + i, 28 + i // 2, (255, 255, 255, 230))
    else:
        for a in range(10, 70):
            rad = math.radians(a)
            for r in range(20, 34):
                x = int(cx + math.cos(rad) * r)
                y = int(cy + math.sin(rad) * r)
                put(img, x, y, (200, 230, 255, 120))
        # sparks
        for sx, sy in ((58, 30), (62, 36), (55, 42), (66, 28), (50, 48)):
            put(img, sx, sy, (255, 255, 200, 255))
            put(img, sx + 1, sy, (255, 220, 120, 200))
    return img


def make_impact() -> Image.Image:
    img = new(32, 32)
    cx = cy = 16
    for y in range(32):
        for x in range(32):
            dx, dy = x - cx, y - cy
            d = math.sqrt(dx * dx + dy * dy)
            if 5 < d < 9:
                put(img, x, y, (255, 255, 230, 230))
            elif d <= 4:
                put(img, x, y, (255, 240, 160, 200))
    for ang in range(0, 360, 45):
        rad = math.radians(ang)
        for r in range(8, 14):
            put(img, int(cx + math.cos(rad) * r), int(cy + math.sin(rad) * r), (255, 255, 255, 220))
    return img


def main():
    for pose, name in [
        ("idle", "player_idle"),
        ("walk_a", "player_walk"),
        ("walk_b", "player_walk_b"),
        ("atk_wind", "player_atk_a"),
        ("atk_slash", "player_atk_b"),
        ("atk_follow", "player_atk_c"),
    ]:
        save(draw_human(new(), pose), name)

    save(make_slash_arc(0), "slash")
    save(make_slash_arc(1), "slash_b")
    save(make_slash_arc(2), "slash_c")
    save(make_impact(), "impact")
    print("done")


if __name__ == "__main__":
    main()
