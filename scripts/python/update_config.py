import os
import yaml
import re
import inflect

p = inflect.engine()

def slugify(text):
    text = text.lower()
    text = re.sub(r'\s+', '-', text)
    text = re.sub(r'[^\w\-]+', '', text)
    text = text.strip('-')
    return text

def normalize_category_name(category_name):
    # Replace spaces and '+' with hyphens
    normalized_name = category_name.replace(' ', '').replace('+', '')
    return normalized_name

def extract_service_name(filename):
    service_name = os.path.splitext(filename)[0]
    service_name = re.sub(r'^[a-z0-9\-]*-icon-service-', '', service_name, flags=re.IGNORECASE)
    service_name = service_name.replace('-', ' ')
    return service_name

def singularize(name):
    return p.singular_noun(name) if p.singular_noun(name) else name

def update_config():
    # Replace the base_path assignment with an absolute path
    base_path = r"c:\Users\scva\source\repos\Azure-PlantUML"
    config_path = os.path.join(base_path, "scripts", "Config.yaml")
    official_icons_path = os.path.join(base_path, "source", "official")
    # manual_icons_path = os.path.join(base_path, "source", "manual") #Not used

    categories = []

    # Process each directory in source/official
    for directory_name in os.listdir(official_icons_path):
        directory_path = os.path.join(official_icons_path, directory_name)

        if os.path.isdir(directory_path):
            # Create category name
            category_name = directory_name.replace('-', ' ').title()
            category_name = normalize_category_name(category_name)

            services = []
            # Process each SVG file in the directory
            for filename in os.listdir(directory_path):
                if filename.lower().endswith(".svg"):
                    # Extract service name
                    service_source = extract_service_name(filename)
                    service_target = service_source.replace(' ', '')
                    service_target = singularize(service_target)

                    services.append({
                        "Source": service_source,
                        "Target": f"{service_target}"
                    })

            categories.append({
                "Name": category_name,
                "Services": services
            })

    # Write to config.yaml
    data = {"Categories": categories}
    with open(config_path, 'w') as outfile:
        yaml.dump(data, outfile, indent=2, sort_keys=False)

if __name__ == "__main__":
    update_config()
