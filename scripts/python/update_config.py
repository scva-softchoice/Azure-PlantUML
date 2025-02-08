import os
import yaml
import re

def slugify(text):
    text = text.lower()
    text = re.sub(r'\s+', '-', text)
    text = re.sub(r'[^\w\-]+', '', text)
    text = text.strip('-')
    return text

def update_config():
    base_path = "/c:/Users/scva/source/repos/Azure-PlantUML"
    config_path = os.path.join(base_path, "scripts", "Config.yaml")
    official_icons_path = os.path.join(base_path, "source", "official")
    manual_icons_path = os.path.join(base_path, "source", "manual")

    # Load existing config
    with open(config_path, 'r') as f:
        config = yaml.safe_load(f)

    # Get all icon file names from official and manual directories
    all_icon_files = []

    for root, _, files in os.walk(official_icons_path):
        for file in files:
            if file.lower().endswith(".svg"):
                all_icon_files.append(file)

    for root, _, files in os.walk(manual_icons_path):
        for file in files:
            if file.lower().endswith(".svg"):
                all_icon_files.append(file)

    # Extract service names from icon file names
    available_services = set()
    for icon_file in all_icon_files:
        service_name = os.path.splitext(icon_file)[0]
        # Remove suffixes like _COLOR, (m)
        service_name = re.sub(r'(_COLOR|\(m\))$', '', service_name, flags=re.IGNORECASE)
        # Replace patterns like *-icon-service-
        service_name = re.sub(r'^[a-z0-9\-]*-icon-service-', '', service_name, flags=re.IGNORECASE)
        available_services.add(service_name)

    # Create a dictionary to map slugified category names to category names
    category_slug_map = {}
    for category in config['Categories']:
        category_slug = slugify(category['Name'])
        category_slug_map[category_slug] = category['Name']

    # Get all subdirectories in source folder
    existing_directories = set()
    for root, dirs, _ in os.walk(official_icons_path):
        for dir in dirs:
            existing_directories.add(dir.lower())

    for root, dirs, _ in os.walk(manual_icons_path):
        for dir in dirs:
            existing_directories.add(dir.lower())

    # Update existing categories
    for category in config['Categories']:
        category_name = category['Name']
        print(f"Processing category: {category_name}")
        services = category['Services'][:]  # Iterate over a copy to allow modification

        for service in services:
            source = service['Source']
            if source not in available_services:
                print(f"  Removing service: {source} from category {category_name}")
                category['Services'].remove(service)

        for service_name in available_services:
            exists = False
            for service in category['Services']:
                if service['Source'] == service_name:
                    exists = True
                    break
            if not exists:
                print(f"  Adding service: {service_name} to category {category_name}")
                new_service = {'Source': service_name, 'Target': f"Azure{service_name.replace(' ', '')}"}
                category['Services'].append(new_service)

    # Find new directories and create categories for them
    existing_category_slugs = set(category_slug_map.keys())
    for directory in existing_directories:
        directory_slug = slugify(directory)
        if directory_slug not in existing_category_slugs:
            print(f"Creating new category for directory: {directory}")
            new_category_name = directory.replace('-', ' ').title()
            new_category = {'Name': new_category_name, 'Services': []}
            config['Categories'].append(new_category)
            category_slug_map[directory_slug] = new_category_name

            # Add services to the new category
            for service_name in available_services:
                print(f"  Adding service: {service_name} to category {new_category_name}")
                new_service = {'Source': service_name, 'Target': f"Azure{service_name.replace(' ', '')}"}
                new_category['Services'].append(new_service)

    # Write updated config back to file
    with open(config_path, 'w') as f:
        yaml.dump(config, f, sort_keys=False)

if __name__ == "__main__":
    update_config()
