import os
import re
import requests
from urllib.parse import urljoin, urlparse

def download_file(url, dest_path):
    response = requests.get(url, stream=True)
    if response.status_code == 200:
        with open(dest_path, 'wb') as f:
            f.write(response.content)
        print(f"Downloaded {os.path.basename(dest_path)}")
    else:
        print(f"Failed to download {url} (Status code: {response.status_code})")

def download_flight_data(kml_url):
    # Extract the flightline id from the KML URL
    kml_file_name = os.path.basename(kml_url)
    match = re.match(r"Browse_Data_(\d{8}_\d{2})\.kml", kml_file_name)
    
    if not match:
        print("Invalid KML URL format.")
        return None  # Return None if the URL format is invalid
    
    flightline_id = match.group(1)
    print(f"Flightline ID: {flightline_id}")

    # Define base URL and the directory structure under 'pipeline/cresis_data'
    parsed_url = urlparse(kml_url)
    base_dir_url = os.path.dirname(kml_url)
    csarp_dir_url = urljoin(base_dir_url, f"CSARP_standard/{flightline_id}/")

    # Set up the file structure inside the 'pipeline/cresis_data' directory
    base_dir = os.path.join(os.getcwd(), 'pipeline', 'cresis_data')
    flight_dir = os.path.join(base_dir, flightline_id)
    mat_dir = os.path.join(flight_dir, "mat")
    mesh_dir = os.path.join(flight_dir, "mesh")
    
    os.makedirs(mat_dir, exist_ok=True)
    os.makedirs(mesh_dir, exist_ok=True)
    
    # Download the KML file
    download_file(kml_url, os.path.join(flight_dir, kml_file_name))
    
    # Download .mat files in a loop until no more files are found
    count = 1
    while True:
        mat_file_url = f"{csarp_dir_url}Data_{flightline_id}_{count:03}.mat"
        mat_file_path = os.path.join(mat_dir, f"Data_{flightline_id}_{count:03}.mat")
        response = requests.head(mat_file_url)
        
        if response.status_code == 200:
            download_file(mat_file_url, mat_file_path)
            count += 1
        else:
            print(f"No more .mat files found after {count-1} files.")
            break
    
    return flightline_id