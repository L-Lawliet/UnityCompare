[toc]

# UnityCompare

> 用于对比Unity3D预制体的工具
> 原理是基于两个Prefab的子树进行依次对比（**基于名字**），然后将差异以两列树结构显示出来。然后点击进入具体的`GameObject`可以看到两个子GameObject的Component差异。再选择具体的GameObject或者Component，可以看到基于`SerializedObject`和`SerializedProperty`的对比。

## 优点

- **可视化的对比两个Prefab的差异，包括结构差异、显示差异、Component个数和属性值差异**
- **与其他对比方案差异主要在于对比窗口是与原Inpector显示是一致的，这样对比和合并时，操作方式与GameObject编辑是一致的。**

## Wiki

- [Home]("https://github.com/L-Lawliet/UnityCompare/wiki/Home")
- [Attention]("https://github.com/L-Lawliet/UnityCompare/wiki/Attention")
- [Definition]("https://github.com/L-Lawliet/UnityCompare/wiki/Definition")
- [Operate]("https://github.com/L-Lawliet/UnityCompare/wiki/Operate")
- [View]("https://github.com/L-Lawliet/UnityCompare/wiki/View")

## 操作演示

![对比操作](/wiki/images/operate/operate_2.gif)

