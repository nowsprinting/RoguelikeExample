# Roguelike Example

[![Meta file check](https://github.com/nowsprinting/RoguelikeExample/actions/workflows/metacheck.yml/badge.svg)](https://github.com/nowsprinting/RoguelikeExample/actions/workflows/metacheck.yml)
[![Test](https://github.com/nowsprinting/RoguelikeExample/actions/workflows/test.yml/badge.svg)](https://github.com/nowsprinting/RoguelikeExample/actions/workflows/test.yml)
[![Integration Test](https://github.com/nowsprinting/RoguelikeExample/actions/workflows/test_integration.yml/badge.svg)](https://github.com/nowsprinting/RoguelikeExample/actions/workflows/test_integration.yml)



## 注意事項

- Unityエディターでしか動作しません
- TextMesh Pro Essentialsをトラッキングから外しているので、最初にPlayするときにインポートを促すウィンドウが出ます。それに従ってインストールしてください



## ゲームプレイ

### キーボード

- hjklyubnキー：移動
- hjklキー + controlもしくはshiftキー同時押しで高速移動（a.k.a. Run, Dash）
- Spaceキー：進行方向に攻撃、選択肢を決定
- Escapeキー：選択ダイアログをキャンセル

### ゲームパッド

- 左スティック：移動
- 左スティック + Eastボタン同時押しで高速移動
- Southボタン：進行方向に攻撃、選択肢を決定
- Eastボタン：選択ダイアログをキャンセル



## テスト

### カテゴリ

一部のテストには、次の `Category` 属性が定義されています。

- **IgnoreCI** : バッチモードでは動作しないテスト
- **Integration** : 統合テスト。カバー範囲が広く実行時間もかかるもので、開発者の手元でのテスト実行およびPull Requestやデフォルトブランチへのマージ契機での実行から除外し、夜間などに実行するためにカテゴライズしています
- **Validation** : アセット・Scene・Prefab・ScriptableObjectなどのバリデーション。失敗したときの通知先が異なる想定でカテゴライズしています



## 設計資料

### インゲーム（行動ターン）のステート遷移図

```mermaid
stateDiagram-v2
    [*] --> PlayerIdol
    PlayerIdol --> PlayerAction
    PlayerAction --> EnemyAction
    EnemyAction --> EnemyPopup
    EnemyPopup --> PlayerIdol: Runでない

    EnemyPopup --> PlayerRun: Runのとき
    PlayerRun --> PlayerAction: 移動先あり
    PlayerRun --> PlayerIdol: 移動先なし（Run解除）

    PlayerAction --> OnStairs: 階段にいる
    OnStairs --> EnemyAction: Cancel
    OnStairs --> PlayerIdol: New Level
    OnStairs --> Result: 地上へ
    Result --> [*]

    (any) --> Result: 死亡
```

### アウトゲームの画面遷移図

```mermaid
stateDiagram-v2
    [*] --> Title

    Title --> StageSelect: Start
    StageSelect --> DifficultySelect
    StageSelect --> Title: Cancel
    DifficultySelect --> Ready?
    DifficultySelect --> StageSelect: Cancel
    Ready? --> DifficultySelect: No
    Ready? --> InGame: Yes
    InGame --> Title

    Title --> Ranking
    Ranking --> Title: Close

    Title --> Option
    Option --> Title: Close

    Title --> Credit
    Credit --> Title: Close

    Title --> Exit?
    Exit? --> Title: No
    Exit? --> [*]: Yes
```



## 権利表記・ライセンス

Copyright (c) 2023 Koji Hasegawa

MIT License


### サードパーティ ライセンス

#### [PixelMplus](https://itouhiro.hatenablog.com/entry/20130602/font)

Copyright (C) 2002-2013 M+ FONTS PROJECT

M+ FONT LICENSE

#### [kgsi/japanese_full.txt](https://gist.github.com/kgsi/ed2f1c5696a2211c1fd1e1e198c96ee4)

ライセンス不明
