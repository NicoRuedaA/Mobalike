import json
import urllib.request
import urllib.parse
import os
import html

json_file = r"F:\Proyectos\Unity\Unity Projects\MobaGameplay\Assets\_Project\Art\Icons\Equipment\items_data.json"
img_dir = r"F:\Proyectos\Unity\Unity Projects\MobaGameplay\Assets\_Project\Art\Icons\Equipment"

with open(json_file, "r", encoding="utf-8") as f:
    items = json.load(f)

headers = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
}

overrides = {
    "Predator&#39;s Paw": "File:PredatorsPaw.png",
    "Lightstep Shoes": "File:LightstepShoes.png",
    "Shadow Treads": "File:ShadowTreads.png",
    "Titan Sabatons": "File:TitanSabatons.png",
}


def download_file(title, filename, name):
    encoded_title = urllib.parse.quote(title)
    api_url = f"https://supervive.wiki.gg/api.php?action=query&titles={encoded_title}&prop=imageinfo&iiprop=url&format=json"

    req = urllib.request.Request(api_url, headers=headers)
    with urllib.request.urlopen(req) as response:
        data = json.loads(response.read().decode("utf-8"))
        pages = data.get("query", {}).get("pages", {})
        for page_id, page_info in pages.items():
            if int(page_id) < 0:
                return False
            imageinfo = page_info.get("imageinfo", [])
            if imageinfo and len(imageinfo) > 0:
                image_url = imageinfo[0].get("url")
                if image_url:
                    out_path = os.path.join(img_dir, filename)
                    print(f"Downloading {name} from {image_url}...")
                    img_req = urllib.request.Request(image_url, headers=headers)
                    with urllib.request.urlopen(img_req) as img_resp:
                        with open(out_path, "wb") as out_f:
                            out_f.write(img_resp.read())
                    return True
    return False


for item in items:
    name = item["name"]
    filename = item["filename"]

    # We only need to fix the ones that are still webp
    out_path = os.path.join(img_dir, filename)
    with open(out_path, "rb") as check_f:
        header = check_f.read(4)
        if header == b"\x89PNG":
            # Already a valid PNG
            continue

    print(f"Fixing {name}...")

    if name in overrides:
        success = download_file(overrides[name], filename, name)
        if success:
            continue

    # Try just title
    success = download_file(f"File:{name}.png", filename, name)
    if success:
        continue

    success = download_file(f"File:{filename}", filename, name)
    if not success:
        print(f"Could not find valid file for {name}")
