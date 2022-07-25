# 粒子系统工具  
ParticleSystem Tool 可以帮助开发者解决屏幕映射面积的问题.  

# 功能  
它可以显示屏幕的总映射面积.  
- 每帧的粒子系统面积。   
- 以及总帧数的平均面积。  
- 屏幕中的粒子总数。  
- 总帧数的平均粒子数。 
 
它同时支持 Billboard 和 Mesh 渲染模式。  
当然你也可以动态开发显示当前帧的最大屏幕投影面积和最大粒子数，都有接口。  

# 用它  
使用起来也非常方便。  
只需将`ParticleWindow.cs`拖到项目的Editor文件中，将`Particle.cs`拖到项目的Scripts文件中，就可以使用了。  

# 代码原理  
## 获取粒子系统  
在UnityEditor中通过拖拽获取ParticlesSystem控件，但类型为GameObject。  

```cs
 var Ptcsys = objs[i].GetComponent<ParticleSystem>();
```

## 递归获取粒子系统中的子系统  
因为一个粒子系统可能有多层子粒子系统，所以需要递归。  

```cs
private void SearchParticleSystemInChildren(ParticleSystem PtcS)  
{
    var PtcSysArray = PtcS.GetComponentsInChildren<ParticleSystem>();
    if（PtcSysArray.Length == 0) return;

    foreach（var p in PtcSysArray）
    {
        if (p != null && list_Ptcsyses.Contains(p) == false)
        {
            list_Ptcsyses.Add(p);
            SearchParticleSystemInChildren(p);
        }
    }
}  
```

## 获取 renderer 模式
渲染模式需要逐层调用底层api。  

```cs
var render = p.GetComponent<Renderer>();
sysRender = render.GetComponent<ParticleSystemRenderer>();
mode = sysRender.renderMode;  

```  

## 获取粒子  

```cs  
private void GetParticles()  
{
    int length = Ptcsys.main.maxParticles;
    particles = new ParticleSystem.Particle[length];
    Ptcsys.GetParticles(particles);
}  

```

## 计算面积

### Billboard模式
在Billboard模式下，因为粒子是 2d。  
只需要得到粒子的位置和当前帧的粒子大小，最后以该位置为中心构建一个正方形，并计算面积。  

获取当前帧粒子的大小  
```cs
var size3d = particles[i].GetCurrentSize(Ptcsys);
```  

### Mesh模式  
在网格模式下，粒子是 3d 对象。  
所以我们应该得到每个粒子的顶点，使用凸包算法得到最外层包的最小顶点，并计算面积。  

#### 部分粒子在屏幕内，部分不在屏幕内  
当 obj 上的顶点数较少时，我们无法准确计算面积。  
所以 ParticleSystemTool 通过两个条件限制了新点进出 obj 来解决这个问题。  
- 在凸包内  
- 屏幕内  

添加点后，再次执行凸包算法计算面积。  
有关详细信息，请参见 Particle.cs 文件中的 `Particle.ParticleSystem_Tools.ParticleSys_Mesh` 类。  
