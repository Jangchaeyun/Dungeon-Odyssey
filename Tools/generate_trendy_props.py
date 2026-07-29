"""Trendy town/dungeon accent props."""
from __future__ import annotations
from pathlib import Path
from PIL import Image, ImageDraw
import math

ROOT = Path(__file__).resolve().parents[1] / "Assets" / "Resources" / "Art"
ROOT.mkdir(parents=True, exist_ok=True)


def save(img, name):
    img.save(ROOT / f"{name}.png")
    print("wrote", name)


def new(w, h):
    return Image.new("RGBA", (w, h), (0, 0, 0, 0))


def put(img, x, y, c):
    if 0 <= x < img.width and 0 <= y < img.height:
        if len(c) == 3:
            c = (*c, 255)
        img.putpixel((x, y), c[:4])


def outline(img, color=(28, 30, 36, 255)):
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


def stall():
    img = new(48, 40)
    # canopy
    for y in range(4, 14):
        for x in range(4, 44):
            stripe = ((x // 4) % 2) == 0
            put(img, x, y, (220, 95, 70, 255) if stripe else (245, 210, 170, 255))
    # posts
    rect(img, 6, 12, 8, 36, (110, 80, 50, 255))
    rect(img, 39, 12, 41, 36, (110, 80, 50, 255))
    # counter
    rect(img, 8, 22, 40, 34, (180, 140, 95, 255))
    rect(img, 10, 18, 38, 24, (90, 160, 140, 255))  # cloth
    return outline(img)


def lantern():
    img = new(16, 28)
    rect(img, 6, 2, 9, 8, (80, 70, 60, 255))
    rect(img, 4, 8, 11, 20, (255, 190, 90, 255))
    rect(img, 5, 9, 10, 18, (255, 230, 140, 230))
    put(img, 7, 12, (255, 255, 220, 255))
    rect(img, 5, 20, 10, 22, (70, 60, 55, 255))
    return outline(img)


def banner():
    img = new(20, 32)
    rect(img, 2, 2, 17, 6, (90, 70, 50, 255))
    for y in range(6, 28):
        for x in range(3, 17):
            put(img, x, y, (40, 140, 130, 255))
    # chevron
    for i in range(6):
        rect(img, 6 + i, 12 + i, 13 - i, 12 + i, (245, 220, 160, 255))
    return outline(img)


def bench():
    img = new(32, 16)
    rect(img, 2, 6, 29, 10, (150, 115, 75, 255))
    rect(img, 4, 10, 7, 14, (110, 80, 50, 255))
    rect(img, 24, 10, 27, 14, (110, 80, 50, 255))
    return outline(img)


def lamp_post():
    img = new(16, 40)
    rect(img, 7, 10, 9, 38, (70, 75, 85, 255))
    rect(img, 4, 4, 12, 12, (255, 210, 120, 255))
    rect(img, 5, 5, 11, 11, (255, 240, 180, 220))
    return outline(img)


def dungeon_rune():
    img = new(24, 24)
    d = ImageDraw.Draw(img)
    d.ellipse([2, 2, 21, 21], outline=(80, 200, 190, 255), width=2)
    d.line([12, 5, 12, 19], fill=(80, 200, 190, 200), width=2)
    d.line([5, 12, 19, 12], fill=(80, 200, 190, 200), width=2)
    return img


def floor_accent():
    img = new(32, 32)
    for y in range(32):
        for x in range(32):
            border = x < 2 or y < 2 or x > 29 or y > 29
            put(img, x, y, (55, 58, 70, 180) if border else (42, 45, 58, 120))
    # inner diamond
    for y in range(8, 24):
        for x in range(8, 24):
            if abs(x - 16) + abs(y - 16) < 8:
                put(img, x, y, (70, 160, 150, 90))
    return img


def town_plaza_decal():
    img = new(48, 48)
    for y in range(48):
        for x in range(48):
            dx, dy = x - 24, y - 24
            d = math.sqrt(dx * dx + dy * dy)
            if 18 < d < 22:
                put(img, x, y, (210, 170, 110, 200))
            elif d < 8:
                put(img, x, y, (90, 170, 155, 160))
    return img


def mist_band():
    img = new(64, 16)
    for y in range(16):
        for x in range(64):
            a = int(40 + 30 * math.sin(x * 0.2) * (1 - abs(y - 8) / 8))
            put(img, x, y, (180, 200, 220, max(0, a)))
    return img


def bookshelf():
    img = new(28, 36)
    rect(img, 2, 2, 25, 34, (90, 65, 45, 255))
    for row in (8, 16, 24):
        rect(img, 3, row, 24, row + 1, (60, 45, 30, 255))
        for i, col in enumerate(((200, 90, 80), (80, 140, 200), (220, 180, 80), (100, 160, 110))):
            rect(img, 4 + i * 5, row - 5, 7 + i * 5, row - 1, (*col, 255))
    return outline(img)


def altar():
    img = new(32, 28)
    rect(img, 6, 14, 25, 26, (70, 72, 85, 255))
    rect(img, 8, 8, 23, 16, (95, 98, 115, 255))
    rect(img, 12, 4, 19, 10, (80, 200, 190, 220))
    return outline(img)


def main():
    save(stall(), "prop_stall")
    save(lantern(), "prop_lantern")
    save(banner(), "prop_banner")
    save(bench(), "prop_bench")
    save(lamp_post(), "prop_lamp")
    save(dungeon_rune(), "prop_rune")
    save(floor_accent(), "prop_floor_accent")
    save(town_plaza_decal(), "prop_plaza_decal")
    save(mist_band(), "prop_mist")
    save(bookshelf(), "prop_bookshelf")
    save(altar(), "prop_altar")
    print("done")


if __name__ == "__main__":
    main()
