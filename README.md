# Dungeon Odyssey

Unity 6 + C# 기반 2D 로그라이크 RPG — **1차 마일스톤**

## 플레이

- **웹 플레이**: https://jangchaeyun.github.io/Dungeon-Odyssey/
- **소스**: https://github.com/Jangchaeyun/Dungeon-Odyssey

## 요구 사항

- Unity **6000.x** (Unity 6)
- 2D / Built-in 또는 기본 렌더 파이프라인

## 첫 실행

1. Unity Hub에서 이 폴더를 프로젝트로 추가·실행합니다.
2. 상단 메뉴 **`Dungeon Odyssey → 1. Setup Project (Scenes + Build Settings)`** 를 실행합니다.
3. 생성된 `Title` 씬에서 **Play** 합니다.

## WebGL 빌드 · GitHub Pages

```
Dungeon Odyssey → Build WebGL (GitHub Pages)
```

산출물: `Builds/WebGL` → `gh-pages` 브랜치로 배포

## 1차 마일스톤 기능

| 기능 | 상태 |
|------|------|
| 타이틀 (새 게임 / 이어하기 / 종료) | ✅ |
| 마을 씬 + NPC 1명 + 던전 입구 | ✅ |
| 랜덤 던전 생성 | ✅ |
| 플레이어 이동 / 기본 공격 | ✅ |
| 슬라임 + HP / 처치 | ✅ |
| 경험치 · 레벨업 | ✅ |
| 던전 클리어 후 마을 복귀 | ✅ |
| JSON 세이브 (1차) | ✅ |
| SQLite (2차) | 예정 |

## 조작

- **WASD / 방향키** : 이동
- **Space / J** : 공격
- **E** : NPC·던전 입구·클리어 포탈 상호작용

## 플레이 흐름

`타이틀 → 새 게임 → 마을(NPC/입구) → 던전(슬라임 전멸) → 포탈 또는 자동 복귀 → 마을`

## 저장 (JSON)

- 경로: `Application.persistentDataPath/Saves/slot_0.json`
- 저장 시점: 새 게임, 마을 입장, 던전 입장/클리어, 종료
- 인터페이스: `ISaveService` → 현재 `JsonSaveService`
- 2차에서 `SqliteSaveService` 구현체로 교체 예정

## 폴더 구조

```
Assets/
  Editor/          프로젝트 자동 세팅
  Scripts/
    Core/          GameManager, 씬 진입점
    Save/          JSON 저장
    Player/        이동·공격·스탯
    Enemy/         슬라임 AI
    Dungeon/       생성·클리어
    Town/          NPC·입구
    UI/            타이틀·HUD
  Scenes/          Setup 메뉴 실행 후 생성
```
