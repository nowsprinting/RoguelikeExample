# Roguelike Example

[![Meta file check](https://github.com/nowsprinting/RoguelikeExample/actions/workflows/metacheck.yml/badge.svg)](https://github.com/nowsprinting/RoguelikeExample/actions/workflows/metacheck.yml)
[![Test](https://github.com/nowsprinting/RoguelikeExample/actions/workflows/test.yml/badge.svg)](https://github.com/nowsprinting/RoguelikeExample/actions/workflows/test.yml)



## 制限事項

- Unityエディターでしか動作しません
- TextMesh Pro Essentialsをignoreしているので、Playするときにインポートを促すウィンドウが出ます。それに従ってインストールしてください



## ゲームプレイ

### キーボード

- hjklyubn：移動（controlもしくはshift同時押しで高速移動）
- space：攻撃



## 設計資料

### 行動ターンのステート遷移図

```mermaid
stateDiagram-v2
    [*] --> PlayerIdol
    PlayerIdol --> PlayerAction
    PlayerAction --> EnemyAction
    EnemyAction --> EnemyPopup
    EnemyPopup --> PlayerIdol: Runでない

    EnemyPopup --> PlayerRun: Runのとき
    PlayerRun --> PlayerAction
    PlayerRun --> PlayerIdol: Runをキャンセル（行き止まりなど）
```
