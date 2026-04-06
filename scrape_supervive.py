import urllib.request
import re
import json
import os

PAGES = {
    "Relics": {
        "url": "https://supervive.wiki.gg/wiki/Relics",
        "slot": "EquipSlot.Chest",
    },
    "Grips": {
        "url": "https://supervive.wiki.gg/wiki/Grips",
        "slot": "EquipSlot.Weapon",
    },
    "Kicks": {"url": "https://supervive.wiki.gg/wiki/Kicks", "slot": "EquipSlot.Boots"},
}

OUTPUT_DIR = "Assets/_Project/Art/Icons/Equipment"
os.makedirs(OUTPUT_DIR, exist_ok=True)

items_data = []
pattern = re.compile(r'<img alt="([^"]+)" src="(/images/thumb/[^"]+)"')

req_headers = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
}

for category, info in PAGES.items():
    print(f"Scraping {category}...")
    try:
        req = urllib.request.Request(info["url"], headers=req_headers)
        with urllib.request.urlopen(req) as response:
            html = response.read().decode("utf-8")

            matches = pattern.findall(html)
            count = 0
            seen_names = set()

            for match in matches:
                item_name = match[0]
                if "logo" in item_name.lower() or "favicon" in item_name.lower():
                    continue
                if item_name in seen_names:
                    continue
                seen_names.add(item_name)

                img_url = (
                    "https://supervive.wiki.gg" + match[1]
                    if not match[1].startswith("http")
                    else match[1]
                )

                safe_name = "".join(
                    c for c in item_name if c.isalnum() or c in (" ", "_")
                ).replace(" ", "")
                filename = f"{safe_name}.png"
                filepath = os.path.join(OUTPUT_DIR, filename)

                try:
                    img_req = urllib.request.Request(img_url, headers=req_headers)
                    with (
                        urllib.request.urlopen(img_req) as img_resp,
                        open(filepath, "wb") as out_file,
                    ):
                        out_file.write(img_resp.read())

                    items_data.append(
                        {"name": item_name, "slot": info["slot"], "filename": filename}
                    )
                    count += 1
                except Exception as e:
                    print(f"  Failed to download {item_name}: {e}")

            print(f"Found {count} items for {category}.")
    except Exception as e:
        print(f"Failed to fetch {category}: {e}")

with open(os.path.join(OUTPUT_DIR, "items_data.json"), "w") as f:
    json.dump(items_data, f, indent=4)
print("Data saved to items_data.json.")
