#!/usr/bin/env python3
"""Apply Cartoon Toon config and convert gameplay Lit/SimpleLit mats to Toon."""

from __future__ import annotations

import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "Assets"

TOON_GUID = "8529db5d8e2d9374cb3e0671e0311c34"
LIT_GUID = "933532a4fcc9baf4fa0491de14d08ed7"
SIMPLE_LIT_GUID = "8d2bb70cbf9db8d4da26e15b26e74248"

SHADE = "0.4"
MIN_LIGHT = "0.12"
MAX_LIGHT = "1"

# Opaque gameplay Lit/SimpleLit materials to convert (relative to Assets/)
CONVERT_RELATIVE = [
    # Player car
    "_Developers/Katsu/Cars/CarMaterials/Giroflex.mat",
    "_Developers/Katsu/Cars/CarMaterials/Giroflex NPC.mat",
    "_Developers/Katsu/Cars/SecretCar/Giroflex2_Blue.mat",
    "_Developers/Katsu/Cars/SecretCar/Giroflex2_Red.mat",
    "_Developers/Katsu/Cars/SecretCar/CarReflex.mat",
    # NPC helpers tied to cars
    "_Developers/Katsu/HelpB.mat",
    "ThirdParty/Characters/mat_0.mat",
    "ThirdParty/Characters/mat_1.mat",
    # Scenery
    "PathCreator/Examples/Materials/Road.mat",
    "ThirdParty/Models/SimpleNaturePack/Materials/SimpleNaturePack_Texture_01.mat",
    "ThirdParty/Models/BigRookGames/Nature Pack/Materials/tree_albedo_01.mat",
    "ThirdParty/Models/BigRookGames/Nature Pack/Materials/leafs.mat",
    "ThirdParty/Models/BigRookGames/Nature Pack/Materials/bush__albedo.mat",
    "ThirdParty/Models/BigRookGames/Nature Pack/Materials/road_borders_dif.mat",
    "ThirdParty/Models/BigRookGames/Nature Pack/Materials/bush_tile.mat",
    "ThirdParty/Models/Fantasy_Kingdom_Pack_Lite/Materials/Tree.mat",
    "ThirdParty/Models/Fantasy_Kingdom_Pack_Lite/Materials/Flower01.mat",
    "ThirdParty/Models/Fantasy_Kingdom_Pack_Lite/Materials/Flower02.mat",
    "ThirdParty/Models/Fantasy_Kingdom_Pack_Lite/Materials/Flower03.mat",
    "ThirdParty/Models/Fantasy_Kingdom_Pack_Lite/Materials/Flower07.mat",
    "ThirdParty/Models/Supercyan Free Forest Sample/Materials/High Quality/Tree/forestpack_tree_fir.mat",
    "ThirdParty/Models/Supercyan Free Forest Sample/Materials/High Quality/Stone/forestpack_stone.mat",
    "ThirdParty/Models/Free Low Poly Desert Pack/Materials/Vertex Color.mat",
    "ThirdParty/Models/Cartoon PalmTree and Umbrella/Palmtree/Materials/Palmtree.mat",
    "ThirdParty/Models/Cartoon_Farm_Crops/Materials/Dirt_Pile.mat",
    # Props / objects
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/Carriage.mat",
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/Pumpkin.mat",
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/Iron_Trim.mat",
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/Rock_Flat_Ma.mat",
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/Stone_Tra.mat",
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/StylizeBark.mat",
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/Trim_wood_0.mat",
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/Barrel_V2.mat",
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/Blocks_1.mat",
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/Stump_Mossy.mat",
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/StylizeBark2.mat",
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/Stylize_Pot_V2.mat",
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/Wall_Lamp.mat",
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/Wood_1.001.mat",
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/Lamp_Light.mat",
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/Fantasy_Tree.mat",
    "ThirdParty/Models/Erbeilo3d_StylizeFreeProps/Stylize_FreeProps/Material/Fantasy_Tree 1.mat",
    "ThirdParty/Models/Fantasy_Kingdom_Pack_Lite/Materials/FK01.mat",
    "ThirdParty/Models/Fantasy_Kingdom_Pack_Lite/Materials/FK02.mat",
    "ThirdParty/Models/LowPolyFarmLite/Materials/LowPolyFarmLite_MAIN.mat",
    "ThirdParty/Models/Low_Poly_Mini_Village/Art/Materials/1.mat",
    "ThirdParty/Models/Low_Poly_Mini_Village/Art/Materials/3.mat",
    "ThirdParty/Models/Cartoon_Farm_Crops/Materials/Eggplant_Plant.mat",
    "ThirdParty/Models/Cartoon_Farm_Crops/Materials/Tomato_Plant.mat",
    "ThirdParty/Models/tractor and planter/models/Materials/tractor_mat.mat",
]

SKIP_NAME_PARTS = (
    "CarWindows",
    "skybox",
    "Skybox",
    "Midgard",
    "LiberationSans",
    "PGDemo",
    "forestpack_foliage",
)


def set_float(text: str, name: str, value: str) -> str:
    pattern = rf"(-\s*{re.escape(name)}:\s*)([-\d.]+)"
    if re.search(pattern, text):
        return re.sub(pattern, rf"\g<1>{value}", text)
    # Insert before m_Colors if possible
    insert = f"    - {name}: {value}\n"
    if "    m_Colors:" in text:
        return text.replace("    m_Colors:", insert + "    m_Colors:", 1)
    if "    m_Floats:\n" in text:
        return text.replace("    m_Floats:\n", "    m_Floats:\n" + insert, 1)
    return text


def extract_tex_block(text: str, prop: str) -> str | None:
    m = re.search(
        rf"(-\s*{re.escape(prop)}:\s*\n(?:[ \t]+.+\n){{1,5}})",
        text,
    )
    return m.group(1) if m else None


def extract_texture_ref(block: str | None) -> tuple[str, str, str] | None:
    if not block:
        return None
    tex = re.search(
        r"m_Texture:\s*(\{fileID:\s*\d+,\s*guid:\s*[0-9a-f]+,\s*type:\s*\d+\})",
        block,
    )
    scale = re.search(r"m_Scale:\s*(\{x:\s*[^,]+,\s*y:\s*[^}]+\})", block)
    offset = re.search(r"m_Offset:\s*(\{x:\s*[^,]+,\s*y:\s*[^}]+\})", block)
    if not tex:
        return None
    return (
        tex.group(1),
        scale.group(1) if scale else "{x: 1, y: 1}",
        offset.group(1) if offset else "{x: 0, y: 0}",
    )


def ensure_texture2d(text: str) -> str:
    base = extract_texture_ref(extract_tex_block(text, "_BaseMap"))
    main = extract_texture_ref(extract_tex_block(text, "_MainTex"))
    src = base or main
    if not src:
        # vertex-color / no texture: keep empty Texture2D
        src = ("{fileID: 0}", "{x: 1, y: 1}", "{x: 0, y: 0}")

    tex_ref, scale, offset = src
    block = (
        f"    - _Texture2D:\n"
        f"        m_Texture: {tex_ref}\n"
        f"        m_Scale: {scale}\n"
        f"        m_Offset: {offset}\n"
    )

    # Replace a well-formed top-level _Texture2D entry only.
    if re.search(r"(?m)^    - _Texture2D:\n", text):
        return re.sub(
            r"(?m)^    - _Texture2D:\n(?:[ \t]+.+\n){1,5}",
            block,
            text,
            count=1,
        )

    # Insert before unity_Lightmaps if present, else after m_TexEnvs
    if "    - unity_Lightmaps:\n" in text:
        return text.replace("    - unity_Lightmaps:\n", block + "    - unity_Lightmaps:\n", 1)
    if "    m_TexEnvs:\n" in text:
        return text.replace("    m_TexEnvs:\n", "    m_TexEnvs:\n" + block, 1)
    return text


def ensure_color_from_base(text: str) -> str:
    base = re.search(r"-\s*_BaseColor:\s*(\{[^}]+\})", text)
    color = re.search(r"-\s*_Color:\s*(\{[^}]+\})", text)
    if base and color:
        return re.sub(r"(-\s*_Color:\s*)(\{[^}]+\})", rf"\g<1>{base.group(1)}", text, count=1)
    if base and not color:
        insert = f"    - _Color: {base.group(1)}\n"
        if "    m_Colors:\n" in text:
            return text.replace("    m_Colors:\n", "    m_Colors:\n" + insert, 1)
    return text


def convert_to_toon(text: str) -> str:
    # Shader line (single or wrapped)
    text = re.sub(
        r"m_Shader:\s*\{fileID:\s*[^}]+\}",
        f"m_Shader: {{fileID: -6465566751694194690, guid: {TOON_GUID}, type: 3}}",
        text,
        count=1,
    )
    text = re.sub(
        r"m_Shader:\s*\{fileID:\s*[-\d]+,\s*guid:\s*[0-9a-f]+,\s*\n\s*type:\s*\d+\}",
        f"m_Shader: {{fileID: -6465566751694194690, guid: {TOON_GUID},\n    type: 3}}",
        text,
        count=1,
    )

    # Clear lit keywords / render tags that don't apply
    text = re.sub(
        r"  m_ValidKeywords:\n(?:  - .+\n)*",
        "  m_ValidKeywords: []\n",
        text,
        count=1,
    )
    text = re.sub(
        r"  stringTagMap:\n(?:    .+\n)*",
        "  stringTagMap: {}\n",
        text,
        count=1,
    )

    text = ensure_texture2d(text)
    text = ensure_color_from_base(text)
    text = set_float(text, "_Shade", SHADE)
    text = set_float(text, "_Min_Light", MIN_LIGHT)
    text = set_float(text, "_Max_Light", MAX_LIGHT)
    return text


def tune_toon(text: str) -> str:
    text = set_float(text, "_Shade", SHADE)
    text = set_float(text, "_Min_Light", MIN_LIGHT)
    text = set_float(text, "_Max_Light", MAX_LIGHT)
    return text


def is_toon(text: str) -> bool:
    return TOON_GUID in text


def is_lit_family(text: str) -> bool:
    return LIT_GUID in text or SIMPLE_LIT_GUID in text


def should_skip(path: Path, text: str) -> bool:
    name = path.name
    if any(p in name or p in str(path) for p in SKIP_NAME_PARTS):
        return True
    if "_SURFACE_TYPE_TRANSPARENT" in text or "RenderType: Transparent" in text:
        return True
    if "m_CustomRenderQueue: 3" in text:  # transparent queues 3000+
        # allow if not transparent keyword; only skip clear glass-like
        if "Transparent" in text:
            return True
    return False


def main() -> None:
    tuned = 0
    converted = 0
    missing = []
    skipped = []

    # 1) Tune every existing Toon material under Assets
    for path in ASSETS.rglob("*.mat"):
        text = path.read_text(encoding="utf-8")
        if not is_toon(text):
            continue
        new = tune_toon(text)
        if new != text:
            path.write_text(new, encoding="utf-8", newline="\n")
            tuned += 1

    # 2) Convert listed gameplay materials
    for rel in CONVERT_RELATIVE:
        path = ASSETS / rel
        if not path.exists():
            missing.append(rel)
            continue
        text = path.read_text(encoding="utf-8")
        if should_skip(path, text):
            skipped.append(rel)
            continue
        if is_toon(text):
            new = tune_toon(text)
            if new != text:
                path.write_text(new, encoding="utf-8", newline="\n")
                tuned += 1
            continue
        if not is_lit_family(text):
            skipped.append(rel + " (not Lit/SimpleLit)")
            continue
        new = convert_to_toon(text)
        path.write_text(new, encoding="utf-8", newline="\n")
        converted += 1

    print(f"Tuned existing Toon mats: {tuned}")
    print(f"Converted Lit/SimpleLit -> Toon: {converted}")
    if missing:
        print("Missing files:")
        for m in missing:
            print(f"  - {m}")
    if skipped:
        print("Skipped:")
        for s in skipped:
            print(f"  - {s}")


if __name__ == "__main__":
    main()
