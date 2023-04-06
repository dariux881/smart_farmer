import os
import numpy as np
from keras.preprocessing.image import load_img, img_to_array
from sklearn.model_selection import train_test_split

# Define the image size
IMG_WIDTH, IMG_HEIGHT = 256, 256

# Define the directory containing the leaf images
DATA_DIR = 'leaf_images/'

# Define the classes (diseases) to be detected
CLASSES = ['healthy', 'disease_1', 'disease_2', 'disease_3']

# Load the images and labels into arrays
images = []
labels = []

for class_id, class_name in enumerate(CLASSES):
    class_dir = os.path.join(DATA_DIR, class_name)
    for filename in os.listdir(class_dir):
        image_path = os.path.join(class_dir, filename)
        image = load_img(image_path, target_size=(IMG_WIDTH, IMG_HEIGHT))
        image = img_to_array(image) / 255.0
        images.append(image)
        labels.append(class_id)

# Convert the data to numpy arrays
images = np.array(images)
labels = np.array(labels)

# Split the data into training and testing sets
train_images, test_images, train_labels, test_labels = train_test_split(images, labels, test_size=0.2, random_state=42)




from keras.models import Sequential
from keras.layers import Conv2D, MaxPooling2D, Flatten, Dense, Dropout
from keras.optimizers import Adam

# Define the model architecture
model = Sequential([
    Conv2D(32, (3, 3), activation='relu', input_shape=(IMG_WIDTH, IMG_HEIGHT, 3)),
    MaxPooling2D((2, 2)),
    Conv2D(64, (3, 3), activation='relu'),
    MaxPooling2D((2, 2)),
    Conv2D(128, (3, 3), activation='relu'),
    MaxPooling2D((2, 2)),
    Flatten(),
    Dense(128, activation='relu'),
    Dropout(0.5),
    Dense(len(CLASSES), activation='softmax')
])

# Compile the model
model.compile(loss='sparse_categorical_crossentropy', optimizer=Adam(lr=0.0001), metrics=['accuracy'])

# Train the model
history = model.fit(train_images, train_labels, epochs=10, batch_size=32, validation_data=(test_images, test_labels))








# Evaluate the model on the test set
test_loss, test_acc = model.evaluate(test_images, test_labels)

print('Test loss:', test_loss)
print('Test accuracy:', test_acc)
