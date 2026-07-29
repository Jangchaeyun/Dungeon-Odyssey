#!/usr/bin/env python3
"""
Meshy Text-to-3D → Assets/Art/AI_Raw 다운로드

준비:
  1) https://www.meshy.ai 에서 API 키 발급 (Pro 이상)
  2) 환경변수 MESHY_API_KEY 설정
  3) 이 스크립트 실행 후 Unity 메뉴:
       Dungeon Odyssey → 3. Apply AI Models

예:
  set MESHY_API_KEY=meshy-xxx
  python Tools/meshy_generate.py
  python Tools/meshy_generate.py --only Player Monster_Slime
"""

from __future__ import annotations

import argparse
import json
import os
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path

API = "https://api.meshy.ai/openapi/v2/text-to-3d"
ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets" / "Art" / "AI_Raw"

# 게임 슬롯명 → 프롬프트 (저폴리, 게임용, 단일 메시)
PROMPTS = {
    "Player": (
        "stylized low poly fantasy adventurer hero, male, leather armor, blue cloak, "
        "holding a sword, game character, T-pose, clean topology, PBR textured, "
        "full body, white background, no base platform"
    ),
    "Monster_Slime": (
        "stylized low poly cave slime monster, green gelatinous body, angry red eyes, "
        "sharp teeth, tentacle arms with claws, spikes on back, game enemy, "
        "clean topology, PBR textured, full body, no platform"
    ),
    "Npc_Mira": (
        "stylized low poly fantasy village guide woman, golden robe, wooden staff with "
        "glowing teal orb, hood, friendly expression, game NPC, T-pose, "
        "clean topology, PBR textured, full body, no platform"
    ),
    "Town_House": (
        "stylized low poly medieval fantasy village house, timber frame, red roof, "
        "door and windows, game asset, clean topology, PBR textured, no people"
    ),
    "Town_Tree": (
        "stylized low poly fantasy oak tree, green canopy, brown trunk, game prop, "
        "clean topology, PBR textured"
    ),
    "Town_Stall": (
        "stylized low poly medieval market stall with canopy, wooden counter, "
        "game prop, clean topology, PBR textured"
    ),
    "Dungeon_Pillar": (
        "stylized low poly dark dungeon stone pillar, cracked mossy stone, "
        "game prop, clean topology, PBR textured"
    ),
    "Dungeon_Torch": (
        "stylized low poly dungeon wall torch with flame, iron bracket, "
        "game prop, clean topology, PBR textured"
    ),
    "Dungeon_Chest": (
        "stylized low poly fantasy treasure chest, copper metal bands, "
        "game prop, clean topology, PBR textured"
    ),
    "Dungeon_Altar": (
        "stylized low poly dungeon stone altar with glowing teal gem, "
        "game prop, clean topology, PBR textured"
    ),
    "Dungeon_Door": (
        "stylized low poly dungeon stone arch doorway with teal magical gate, "
        "game prop, clean topology, PBR textured"
    ),
    "Dungeon_Gate": (
        "stylized low poly large dungeon entrance gate with copper lintel, "
        "stone pillars, purple gate door, game prop, clean topology, PBR textured"
    ),
    "Exit_Portal": (
        "stylized low poly magical circular portal ring with cyan energy core, "
        "game prop, clean topology, PBR textured, vertical ring"
    ),
}


def request(method: str, url: str, api_key: str, body: dict | None = None) -> dict:
    data = None if body is None else json.dumps(body).encode("utf-8")
    req = urllib.request.Request(
        url,
        data=data,
        method=method,
        headers={
            "Authorization": f"Bearer {api_key}",
            "Content-Type": "application/json",
            "Accept": "application/json",
        },
    )
    try:
        with urllib.request.urlopen(req, timeout=120) as res:
            raw = res.read().decode("utf-8")
            return json.loads(raw) if raw else {}
    except urllib.error.HTTPError as e:
        err = e.read().decode("utf-8", errors="replace")
        raise RuntimeError(f"HTTP {e.code}: {err}") from e


def download(url: str, dest: Path) -> None:
    dest.parent.mkdir(parents=True, exist_ok=True)
    with urllib.request.urlopen(url, timeout=180) as res, open(dest, "wb") as f:
        f.write(res.read())


def create_preview(api_key: str, prompt: str) -> str:
    body = {
        "mode": "preview",
        "prompt": prompt,
        "art_style": "sculpture",
        "should_remesh": True,
        "target_formats": ["glb", "fbx"],
    }
    # art_style: realistic / sculpture 등 — 문서에 맞게 조정
    try:
        result = request("POST", API, api_key, body)
    except RuntimeError:
        body.pop("art_style", None)
        result = request("POST", API, api_key, body)
    task_id = result.get("result") or result.get("id")
    if not task_id:
        raise RuntimeError(f"preview 응답에 task id 없음: {result}")
    return task_id


def wait_task(api_key: str, task_id: str, label: str) -> dict:
    url = f"{API}/{task_id}"
    while True:
        task = request("GET", url, api_key)
        status = task.get("status", "")
        progress = task.get("progress", 0)
        print(f"  [{label}] {status} {progress}%")
        if status == "SUCCEEDED":
            return task
        if status in ("FAILED", "CANCELED"):
            raise RuntimeError(f"{label} 실패: {task}")
        time.sleep(4)


def create_refine(api_key: str, preview_id: str) -> str:
    body = {
        "mode": "refine",
        "preview_task_id": preview_id,
        "target_formats": ["glb", "fbx"],
        "enable_pbr": True,
    }
    result = request("POST", API, api_key, body)
    task_id = result.get("result") or result.get("id")
    if not task_id:
        raise RuntimeError(f"refine 응답에 task id 없음: {result}")
    return task_id


def generate_one(api_key: str, name: str, prompt: str, skip_refine: bool) -> None:
    print(f"\n=== {name} ===")
    print(f"prompt: {prompt[:80]}...")
    preview_id = create_preview(api_key, prompt)
    preview = wait_task(api_key, preview_id, f"{name}/preview")

    task = preview
    if not skip_refine:
        refine_id = create_refine(api_key, preview_id)
        task = wait_task(api_key, refine_id, f"{name}/refine")

    urls = task.get("model_urls") or {}
    saved = []
    for fmt in ("fbx", "glb"):
        url = urls.get(fmt)
        if not url:
            continue
        dest = OUT / f"{name}.{fmt}"
        print(f"  download {fmt} → {dest}")
        download(url, dest)
        saved.append(dest)

    if not saved:
        raise RuntimeError(f"{name}: model_urls 비어 있음: {task}")


def main() -> int:
    parser = argparse.ArgumentParser(description="Meshy → AI_Raw")
    parser.add_argument("--only", nargs="*", help="생성할 슬롯명만")
    parser.add_argument("--skip-refine", action="store_true", help="텍스처 refine 생략(크레딧 절약)")
    parser.add_argument("--list", action="store_true", help="슬롯 목록만 출력")
    args = parser.parse_args()

    if args.list:
        for k in PROMPTS:
            print(k)
        return 0

    api_key = os.environ.get("MESHY_API_KEY", "").strip()
    if not api_key:
        print("MESHY_API_KEY 환경변수가 없습니다.", file=sys.stderr)
        print("  Windows:  set MESHY_API_KEY=meshy-xxxx", file=sys.stderr)
        print("  https://www.meshy.ai/api 에서 키를 발급하세요.", file=sys.stderr)
        return 1

    names = args.only if args.only else list(PROMPTS.keys())
    OUT.mkdir(parents=True, exist_ok=True)

    for name in names:
        if name not in PROMPTS:
            print(f"알 수 없는 슬롯: {name}", file=sys.stderr)
            return 1
        generate_one(api_key, name, PROMPTS[name], args.skip_refine)

    print("\n완료. Unity에서 메뉴 실행:")
    print("  Dungeon Odyssey → 3. Apply AI Models (AI_Raw → Resources)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
