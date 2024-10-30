import os
from PIL import Image
def flip_images(folder_path):
    # Iterate over all files in the folder
    for filename in os.listdir(folder_path):
        # Check if the file is an image
        if filename.lower().endswith(('.png', '.jpg', '.jpeg', '.gif')):
            # Open the image
            image_path = os.path.join(folder_path, filename)
            with Image.open(image_path) as image:
                # Flip the image vertically
                flipped_image = image.transpose(Image.FLIP_TOP_BOTTOM)
                
                # Save the flipped image with the same name
                flipped_image.save(image_path)
                
                print(f"Flipped {filename}")
# Specify the folder path containing the images
folder_path = r'D:\GIT\polAR\PolXR\Assets\Resources\Radar3D\HorizontalRadar'
# Call the function to flip the images
flip_images(folder_path)