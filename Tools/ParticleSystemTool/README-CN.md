# ����ϵͳ����  
ParticleSystem Tool ���԰��������߽����Ļӳ�����������.  

# ����  
��������ʾ��Ļ����ӳ�����.  
- ÿ֡������ϵͳ�����   
- �Լ���֡����ƽ�������  
- ��Ļ�е�����������  
- ��֡����ƽ����������  
��ͬʱ֧�� Billboard �� Mesh ��Ⱦģʽ��  
��Ȼ��Ҳ���Զ�̬������ʾ��ǰ֡�������ĻͶӰ�������������������нӿڡ�  

# ����  
ʹ������Ҳ�ǳ����㡣  
ֻ�轫`ParticleWindow.cs`�ϵ���Ŀ��Editor�ļ��У���`Particle.cs`�ϵ���Ŀ��Scripts�ļ��У��Ϳ���ʹ���ˡ�  

# ����ԭ��  
## ��ȡ����ϵͳ  
��UnityEditor��ͨ����ק��ȡParticlesSystem�ؼ���������ΪGameObject��  

cs```
 var Ptcsys = objs[i].GetComponent<ParticleSystem>();
```

##�ݹ��ȡ����ϵͳ�е���ϵͳ  
��Ϊһ������ϵͳ�����ж��������ϵͳ��������Ҫ�ݹ顣  

cs```
private void SearchParticleSystemInChildren(ParticleSystem PtcS)  
{
    var PtcSysArray = PtcS.GetComponentsInChildren<ParticleSystem>();
    �����PtcSysArray.Length == 0�����أ�

    foreach��PtcSysArray �еı��� p��
    {
        if (p != null && list_Ptcsyses.Contains(p) == false)
        {
            list_Ptcsyses.Add(p);
            SearchParticleSystemInChildren(p);
        }
    }
}  
```

## ��ȡ renderer ģʽ
��Ⱦģʽ��Ҫ�����õײ�api��  

cs```  
var render = p.GetComponent<Renderer>();
sysRender = render.GetComponent<ParticleSystemRenderer>();
mode = sysRender.renderMode;  

```  

## ��ȡ����  

cs```  
private void GetParticles()  
{
    int length = Ptcsys.main.maxParticles;
    particles = new ParticleSystem.Particle[length];
    Ptcsys.GetParticles(particles);
}  

```

##�������

### Billboardģʽ
��Billboardģʽ�£���Ϊ������ 2d��  
ֻ��Ҫ�õ����ӵ�λ�ú͵�ǰ֡�����Ӵ�С������Ը�λ��Ϊ���Ĺ���һ�������Σ������������  

��ȡ��ǰ֡���ӵĴ�С  
cs```
var size3d = particles[i].GetCurrentSize(Ptcsys);

```  

### Meshģʽ  
������ģʽ�£������� 3d ����  
��������Ӧ�õõ�ÿ�����ӵĶ��㣬ʹ��͹���㷨�õ�����������С���㣬�����������  

#### ������������Ļ�ڣ����ֲ�����Ļ��  
�� obj �ϵĶ���������ʱ�������޷�׼ȷ���������  
���� ParticleSystemTool ͨ�����������������µ���� obj �����������⡣  
- ��͹����  
- ��Ļ��  
��ӵ���ٴ�ִ��͹���㷨���������  
�й���ϸ��Ϣ����μ� Particle.cs �ļ��е� `Particle.ParticleSystem_Tools.ParticleSys_Mesh` �ࡣ  