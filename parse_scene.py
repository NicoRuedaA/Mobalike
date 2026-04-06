import re

scene_file = "Assets/_Project/Scenes/SampleScene.unity"

with open(scene_file, 'r', encoding='utf-8') as f:
    lines = f.readlines()

# Find Player GameObject
player_go_line = -1
for i, line in enumerate(lines):
    if line.strip() == "m_Name: Player":
        player_go_line = i
        break

if player_go_line == -1:
    print("Player not found")
    exit()

go_id = ""
for i in range(player_go_line, -1, -1):
    if lines[i].startswith("--- !u!1 &"):
        go_id = lines[i].split("&")[1].strip()
        break

print(f"Player ID: {go_id}")

# Get all components
components = []
in_components = False
for i in range(player_go_line, -1, -1):
    if "m_Component:" in lines[i]:
        for j in range(i+1, player_go_line):
            if "- component:" in lines[j]:
                match = re.search(r"fileID: (\d+)", lines[j])
                if match:
                    components.append(match.group(1))
        break

print(f"Components: {components}")

# Find component details
for cid in components:
    for i, line in enumerate(lines):
        if line.startswith(f"--- !u!") and f"&{cid}" in line:
            type_id = line.split("!u!")[1].split()[0]
            print(f"\nFound Component {cid} (Type !u!{type_id}):")
            for j in range(i+1, i+15):
                if lines[j].startswith("--- !u!"): break
                print("  " + lines[j].strip())

