# Valve Resource Format

[![Build Status (Github)](https://github.com/SteamDatabase/ValveResourceFormat/workflows/.NET%20Core%20CI/badge.svg)](https://github.com/SteamDatabase/ValveResourceFormat/actions)
[![Build Status (AppVeyor)](https://img.shields.io/appveyor/ci/xPaw/valveresourceformat/master.svg?label=AppVeyor)](https://ci.appveyor.com/project/xPaw/valveresourceformat)
[![Coverage Status](https://img.shields.io/coveralls/SteamDatabase/ValveResourceFormat.svg?label=Test%20Coverage)](https://coveralls.io/github/SteamDatabase/ValveResourceFormat)
[![NuGet](https://img.shields.io/nuget/v/ValveResourceFormat.svg?label=NuGet)](https://www.nuget.org/packages/ValveResourceFormat/)

Valve's Source 2 resource file format *(files that usually end with `_c` like `.vmdl_c`)* parser and decompiler. Contents of this repository are available under [MIT](LICENSE) license.

**Interested in helping? Jump in `#steamdb` on [freenode](https://freenode.net/) and ask away!**

This repository is split into three components:
- *CLI Decompiler* - File viewer, decompiler and a playground for testing new formats and features.
- *GUI Viewer* - A complete mess of winforms and other fun things.
- *Library* - The only sane part of this repository, provides public API.

You can download bleeding edge builds [from AppVeyor](https://ci.appveyor.com/project/xPaw/valveresourceformat/build/artifacts).

## What's supported?
- Model viewer
- Map viewer
- Sound player
- VPK viewer which supports opening and exporting files
- Read only VPK API
- Binary KeyValues3 parser
- NTRO support

### Supported resource types
Ext      | Name                    | Support
-------- | ----------------------- | -------
vanim    | Animation               | No
vagrp    | Animation Group         | No
vseq     | Sequence Group          | No
vpcf     | Particle System         | :+1: NTRO, KV3
vmat     | Material                | :+1: NTRO
vmks     | Sheet                   | No
vmesh    | Mesh                    | Vertex and index buffers, vertex attributes
vtex     | Compiled Texture        | :+1: DXT1, DXT5, I8, RGBA8888, R16, RG1616, RGBA16161616, R16F, RG1616F, RGBA16161616F, R32F, RG3232F, RGB323232F, RGBA32323232F, IA88, PNG, JPG, ETC2, ETC2_EAC, BGRA8888
vmdl     | Model                   | Started
vphys    | Physics Collision Mesh  | No
vsnd     | Sound                   | :+1: wav, mp3
vmorf    | MorphSet                | No
vrman    | ResourceManifest        | :+1:
vwrld    | World                   | :+1:
vwnod    | WorldNode               | :+1:
vvis     | WorldVisibility         | No
vents    | EntityLump              | No
vsurf    | Surface Properties      | No
vsndevts | Sound Event Script      | :+1: KV1, :-1: KV3
vsndstck | Sound Stack Script      | :+1: KV1, :-1: KV3
vrmap    | Resource Remap Table    | No
vcss     | Panorama Style          | :+1:
vxml     | Panorama Layout         | :+1:
vpdi     | Panorama Dynamic Images | No
vjs      | Panorama Script         | :+1:
vsvg     | Panorama Vector Graphic | :+1:
vpsf     | Particle Snapshot       | No
vmap     | Map                     | :+1:

Ext      | Name                    | Support
-------- | ----------------------- | -------
vpk      | Pak (package)           | :+1: Handled by [ValvePak](https://github.com/SteamDatabase/ValvePak)
vcs      | Compiled Shader         | Partially supported by `CompiledShader`
vfont    | Bitmap Font             | :+1: Decrypts `VFONT1`, supported in Source 1 (CS:GO) and Source 2 (Dota 2).

Not all formats are 100% supported, some parameters are still unknown and not fully understood.

## Eye catchy screenshots
<table>
	<tr>
		<td><img src="https://steamdatabase.github.io/ValveResourceFormat/static/screen_glados.png"></td>
		<td><img src="https://steamdatabase.github.io/ValveResourceFormat/static/screen_potato.png"></td>
	</tr>
	<tr>
		<td><img src="https://steamdatabase.github.io/ValveResourceFormat/static/screen_vpk.png"></td>
		<td><img src="https://steamdatabase.github.io/ValveResourceFormat/static/screen_cli.png"></td>
	</tr>
</table>
