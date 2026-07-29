"""Hurt / death reaction frames + polished UI bits."""
from __future__ import annotations

from pathlib import Path
from PIL import Image, ImageDraw
import math

ROOT = Path(__file__).resolve().parents[1] / "Assets" / "Resources" / "Art"
ROOT.mkdir(parents=True, exist_ok=True)


def save(img, name):
    path = ROOT / f"{name}.png"
    img.save(path)
    print("wrote", path)


def new(w=64, h=64):
    return Image.new("RGBA", (w, h), (0, 0, 0, 0))


def put(img, x, y, c):
    if 0 <= x < img.width and 0 <= y < img.height:
        if isinstance(c, tuple) and len(c) == 3:
            c = (*c, 255)
        elif isinstance(c, tuple) and len(c) > 4:
            c = c[:4]
        img.putpixel((x, y), c)


def blend(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3)) + (255,)


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


def rect(img, x0, y0, x1, y1, c):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            put(img, x, y, c)


def disk(img, cx, cy, rx, ry, base, light, dark):
    for y in range(int(cy - ry - 1), int(cy + ry + 2)):
        for x in range(int(cx - rx - 1), int(cx + rx + 2)):
            nx = (x - cx) / max(rx, 1)
            ny = (y - cy) / max(ry, 1)
            if nx * nx + ny * ny > 1:
                continue
            lum = max(0, min(1, 0.55 - 0.3 * nx - 0.45 * ny))
            c = blend(dark, light, lum) if lum < 0.55 else blend(base, light, (lum - 0.55) / 0.45)
            put(img, x, y, c)


def draw_human_hurt():
    """Recoil pose — leaning back, eyes closed, sword dropped angle."""
    img = new()
    # shadow
    for y in range(54, 60):
        for x in range(18, 46):
            if (x - 32) ** 2 / 144 + (y - 56) ** 2 / 9 <= 1:
                put(img, x, y, (0, 0, 0, 70))

    bx = 30  # lean back
    # legs
    rect(img, 22, 42, 27, 54, (55, 70, 110, 255))
    rect(img, 34, 43, 39, 54, (55, 70, 110, 255))
    rect(img, 21, 52, 28, 55, (60, 45, 38, 255))
    rect(img, 33, 52, 40, 55, (60, 45, 38, 255))
    # torso
    for y in range(24, 40):
        for x in range(bx - 9, bx + 8):
            put(img, x, y, blend((55, 115, 185), (110, 175, 235), 0.4))
    rect(img, bx - 9, 36, bx + 7, 38, (190, 145, 55, 255))
    # head
    disk(img, bx, 14, 8, 8, (235, 190, 150), (250, 220, 190), (185, 140, 105))
    disk(img, bx, 11, 8, 6, (75, 48, 32), (120, 80, 50), (45, 28, 18))
    # closed eyes (pain)
    rect(img, bx - 4, 14, bx - 1, 15, (60, 40, 40, 255))
    rect(img, bx + 1, 14, bx + 4, 15, (60, 40, 40, 255))
    # open mouth
    rect(img, bx - 1, 18, bx + 2, 20, (80, 30, 30, 255))
    # red flash tint overlay on body edge
    put(img, bx - 10, 28, (255, 80, 80, 160))
    put(img, bx + 8, 30, (255, 80, 80, 160))
    # arms up defending
    rect(img, bx - 14, 22, bx - 10, 32, (220, 175, 140, 255))
    rect(img, bx + 7, 22, bx + 12, 30, (60, 120, 190, 255))
    # sword angled down
    for i in range(16):
        put(img, bx + 12 + i // 2, 32 + i, (210, 220, 235, 255))
    return outline(img)


def draw_slime_hurt():
    img = new(48, 48)
    for y in range(40, 46):
        for x in range(10, 38):
            if (x - 24) ** 2 / 140 + (y - 42) ** 2 / 16 <= 1:
                put(img, x, y, (0, 0, 0, 60))
    # flattened / squashed
    cy, rx, ry = 28, 16, 9
    for y in range(48):
        for x in range(48):
            nx, ny = (x - 24) / rx, (y - cy) / ry
            if nx * nx + ny * ny > 1:
                continue
            lum = max(0, min(1, 0.5 - 0.3 * nx - 0.4 * ny))
            c = blend((25, 110, 55), (200, 255, 180), lum)
            # red hurt tint
            c = blend(c[:3], (255, 100, 100), 0.25)
            put(img, x, y, c)
    # X eyes
    for dx in (-6, 6):
        put(img, 24 + dx, 26, (40, 20, 20, 255))
        put(img, 23 + dx, 25, (40, 20, 20, 255))
        put(img, 25 + dx, 25, (40, 20, 20, 255))
        put(img, 23 + dx, 27, (40, 20, 20, 255))
        put(img, 25 + dx, 27, (40, 20, 20, 255))
    # drip
    rect(img, 20, 34, 22, 38, (70, 200, 100, 200))
    return outline(img)


def draw_slime_death():
    img = new(48, 48)
    # puddle
    for y in range(48):
        for x in range(48):
            nx, ny = (x - 24) / 18, (y - 36) / 6
            if nx * nx + ny * ny <= 1:
                put(img, x, y, (40, 140, 70, 200))
    # bubbles
    for cx, cy, r in [(16, 30, 3), (28, 28, 4), (22, 24, 2)]:
        for y in range(cy - r, cy + r + 1):
            for x in range(cx - r, cx + r + 1):
                if (x - cx) ** 2 + (y - cy) ** 2 <= r * r:
                    put(img, x, y, (100, 220, 130, 180))
    return img


def hit_spark():
    img = new(32, 32)
    cx = cy = 16
    for ang in range(0, 360, 30):
        rad = math.radians(ang)
        for r in range(4, 14):
            x = int(cx + math.cos(rad) * r)
            y = int(cy + math.sin(rad) * r)
            a = int(255 * (1 - r / 14))
            put(img, x, y, (255, 255, 220, a))
    for y in range(12, 21):
        for x in range(12, 21):
            put(img, x, y, (255, 240, 160, 230))
    return img


def ui_panel():
    img = new(64, 64)
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([1, 1, 62, 62], radius=10, fill=(18, 22, 34, 230), outline=(212, 175, 85, 255), width=2)
    d.rounded_rectangle([4, 4, 59, 59], radius=8, fill=(28, 34, 52, 120), outline=(90, 130, 180, 80), width=1)
    return img


def ui_button():
    img = new(128, 40)
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([1, 1, 126, 38], radius=8, fill=(32, 42, 64, 245), outline=(220, 180, 80, 255), width=2)
    d.rounded_rectangle([4, 4, 123, 18], radius=4, fill=(255, 255, 255, 18))
    return img


def main():
    save(draw_human_hurt(), "player_hurt")
    save(draw_slime_hurt(), "slime_hurt")
    save(draw_slime_death(), "slime_death")
    save(hit_spark(), "hit_spark")
    save(ui_panel(), "ui_panel")
    save(ui_button(), "ui_button")
    print("done")


if __name__ == "__main__":
    main()
