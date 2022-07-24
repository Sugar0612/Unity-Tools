# Language  
[中文简体](.//README-CN.md)

# ParticleSystem Tool  
ParticleSystem Tool can help developers solve the problem of screen mapping area.  

# Function  
It can display the total screen mapped area of the  
- ParticleSystem for each frame.  
- along with the average area over the total number of frames.  
- the total number of particles in the screen.  
- the average particle count over the total number of frames.  

It supports both Billboard and Mesh rendering modes.  
Of course You can also dynamically develop it to display the maximum screen projection area and maximum number of particles of the current frame, all with interfaces.  

# Use it  
It is also very convenient to use.  
just drag `ParticleWindow.cs` to the Editor file of the project, drag `Particle.cs` to the Scripts file of the project, and you can use it.  

# Principle  
## Get particle system  
Get the ParticlesSystem by dragging and dropping in UnityEditor, but the type is GameObject.  

```cs  
 var Ptcsys = objs[i].GetComponent<ParticleSystem>();  
```  

## Get the particle system neutron particle system recursively,  
Because it is possible for a particle system to have multiple layers of child particle systems, recursion is required.  

```cs  
private void SearchParticleSystemInChildren(ParticleSystem PtcS)  
{
    var PtcSysArray = PtcS.GetComponentsInChildren<ParticleSystem>();
    if (PtcSysArray.Length == 0) return;

    foreach(var p in PtcSysArray)
    {
        if (p != null && list_Ptcsyses.Contains(p) == false)
        {
            list_Ptcsyses.Add(p);
            SearchParticleSystemInChildren(p);
        }
    }
}
```  

##  Get the rnenderer mode  
The rendering mode needs to call the underlying api layer by layer.

```cs  
var render = p.GetComponent<Renderer>();
sysRender = render.GetComponent<ParticleSystemRenderer>();
mode = sysRender.renderMode;  

```  

## get particle  

```cs  
private void GetParticles()  
{
    int length = Ptcsys.main.maxParticles;
    particles = new ParticleSystem.Particle[length];
    Ptcsys.GetParticles(particles);
}  

```cs  
## calculate area  

### Billboard mode  
In billboard rendering mode.  
because the particle is a 2d, you only need to get the position of the particle and the particle size of the current frame, and finally build a square with the position as the center, and calculate the area.  

Get the size of the current frame particle  
```cs
var size3d = particles[i].GetCurrentSize(Ptcsys);  
```  

### Mesh mode  
In mesh mode, the particle is a 3d obj.  
so we should get the vertex of each particle, use the convex hull algorithm to get the smallest vertex of the outermost package, and calculate the area.  

#### Part of the particle is inside the screen, part of it is not  
When the number of vertices on obj is small, we cannot accurately calculate the area.  
So ParticleSystemTool solves this problem by limiting the addition of new points to and from obj through two conditions.  
- within the convex hull  
- within the screen  

After adding points, perform the convex hull algorithm again to calculate the area.  
For details, see the `Particle.ParticleSystem_Tools.ParticleSys_Mesh` class in Particle.cs file.
