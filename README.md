# UniWinApi Example project

UniWinApi は Unityでは本来行えない操作を Windows API 経由で行うものです。  
以下のようなことができます。  

* ウィンドウの移動
* ウィンドウサイズ変更
* ウィンドウの最大化、最小化
* **ウィンドウの透過** （枠なしで、四角形でないウィンドウにします） 
* **ファイルのドロップを受け付ける**
* **Windowsのダイアログでファイルを開く（試験実装で単一ファイルのみ）**
* マウスポインタを移動させる
* マウスのボタン操作を送出する

主にデスクトップマスコット的な用途で利用しそうな機能を取り込んでいます。

このリポジトリではそれらの機能を利用したデスクトップマスコット風のVRMビューアーのプロジェクトを置いてあります。  
[![UniWinApi VRM viewer](http://i.ytimg.com/vi/cq2g-hIGlAs/mqdefault.jpg)](https://youtu.be/cq2g-hIGlAs "UniWinApi VRM viewer v0.4.0 beta")


## Download

ビルド済みのVRMビューア―例は [Releases](https://github.com/kirurobo/UniWinApi/releases) 中の UniWinApiVrmViewer です。64ビット版と32ビット版(x86)があります。
* [Ver.0.4.0-beta 色々改造](https://github.com/kirurobo/UniWinApi/releases/tag/v0.4.0beta)
* [Ver.0.3.3 UniVRM 0.44に](https://github.com/kirurobo/UniWinApi/releases/tag/v0.3.3)
* [Ver.0.3.2 マウスを追う](https://github.com/kirurobo/UniWinApi/releases/tag/v0.3.2)
* [Ver.0.3.1 最初から透明化](https://github.com/kirurobo/UniWinApi/releases/tag/v0.3.1)
* [Ver.0.3.0 照明の回転と並進移動も追加](https://github.com/kirurobo/UniWinApi/releases/tag/v0.3.0)
* [Ver.0.2.3 UniVRM 0.42に。カメラFOVを10度に](https://github.com/kirurobo/UniWinApi/releases/tag/v0.2.3)
* [Ver.0.2.2 ライトを白色に](https://github.com/kirurobo/UniWinApi/releases/tag/v0.2.2)
* [Ver.0.2.1 シェーダー修正後](https://github.com/kirurobo/UniWinApi/releases/download/v0.2.1/UniWinApiVrmViewer_x64_v0.2.1.zip)
* [Ver.0.2.0 初版](https://github.com/kirurobo/UniWinApi/releases/download/v0.2.0/UniWinApiVrmViewer_x64.zip)


## License

UniWinApi本体は [![CC0](http://i.creativecommons.org/p/zero/1.0/88x31.png "CC0")](http://creativecommons.org/publicdomain/zero/1.0/deed.ja) です。  
ただしVRMビューアはそれ以外のコードを含みます。

## System requirements

* Unity 5.6 or newer
* Windows 7 or newer

## Usage

[日本語版のチュートリアルはこちら](docs/index_jp.md)

## Configuration

本体は Assets/UniWinApi 以下です。  
このリポジトリには使用例として DWANGO Co., Ltd. による UniVRM を含んでいます。

<br />
<br />

---

# About [UniVRM](https://github.com/dwango/UniVRM/releases) 

## License

* [MIT License](Assets/VRM/LICENSE.txt)

## About [VRM](https://dwango.github.io/vrm/)
###
"VRM" is a file format for using 3d humanoid avatars (and models) in VR applications.  
VRM is based on glTF2.0. And if you comply with the MIT license, you are free to use it.  
