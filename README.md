# Voxel Example 

1.) Convert Mesh to Voxels

2.) Render converted voxels using shaders, implement ray marching in shader

3.) LOD setting for voxel generation


# How to use it

1.) Open Sample Scene

2.) Run the game & it will render a capsule. 

3.) A empty object with Voxelize script is added to scene, this script is responsible for voxelizing a given mesh (in this case Capsule) & then rendering it using custom shader. 

# Development challenges

1.) Faced bit of challenge to figure out how to iterate through Unity's mesh implementation

2.) Used triangle intersection to figure out collision with various voxels. It was bit challenging since I needed to figure out correct math, however, this is slow and should be optimized using octtree. This does however gives us opporunity to run the algorithm using compute shader. 
