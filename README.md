# AtlasChanger-Unity
A Unity Editor window that helps you to refactor the images of a game object and it's children's images.

Atlas Changer, is a simple tool that i created for re allocating UI images to different atlases to optimize drawcalls.

Basically it caches every sprite name used by the given game object or it's children and then looks for it in given atlases respective to the order of atlases in the reorderable list. Higher it is, higher the priority. If it find's the sprite in atlas it switches the old sprite with the one in the atlas, both ways it logs the found and changed sprites and missing ones where you can click to the button next to the list element and check out the gameobjects concerning that list element.
