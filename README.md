[toc]

# UnityCompare

> 用于对比Unity3D预制体的工具
> 原理是基于两个Prefab的子树进行依次对比（**基于名字**），然后将差异以两列树结构显示出来。然后点击进入具体的`GameObject`可以看到两个子GameObject的Component差异。再选择具体的GameObject或者Component，可以看到基于`SerializedObject`和`SerializedProperty`的对比。

## 优点

- **可视化的对比两个Prefab的差异，包括结构差异、显示差异、Component个数和属性值差异**
- **与其他对比方案差异主要在于对比窗口是与原Inpector显示是一致的，这样对比和合并时，操作方式与GameObject编辑是一致的。**

## 界面介绍

### Compare界面

> 菜单：Tools/Compare/CompareWindow

用于显示两个Prefab或者GameObject的差异列表。

![Compare界面](/wiki/images/1.jpg)

#### 菜单栏
- Compare按钮：点击后会重新对比两个预制体（均不为空时）的差异
![Compare按钮](/wiki/images/2.jpg)
- Equal项按钮：用于开关开启“相等”项的显示
![Equal项按钮](/wiki/images/3.jpg)
- Miss项按钮：用于开关开启“缺失”项的显示
![Miss项按钮](/wiki/images/4.jpg)

#### GameObject列表

- Prefab项：选择需要对比的Prefab
- 列表：显示GameObject的树状结构。双击节点显示对应Component列表。单击可以刷新Inpector界面。
![GameObject列表](/wiki/images/5.jpg)

#### Component列表

- Title：显示GameObject名称，点击可以返回GameObject列表
- Component列表：显示Component对比差异。单击可以刷新Inpector界面。
![Component列表](/wiki/images/6.jpg)

### Inpector界面

- Inpector：显示GameObject和Component的信息
- 警告栏：会显示具体不相等的字段名
![Inpector界面](/wiki/images/7.jpg)

## 操作

![差异类型](/wiki/images/8.gif)

1. 点击菜单：Tools/Compare/CompareWindow
2. 分别拖拽需要对比的预制体到CompareWindow的Prefab项中
3. Prefab更改后，会自动触发Compare，刷新GameObject列表
4. 根据列表的图标可知Prefab的差异
5. 点击相应的差异项，显示其Inspector界面（属性差异）
6. 根据警告栏得到具体的属性差异信息
7. 修改属性差异，并保存。
8. 点击Compare按钮重新对比。

## 差异类型

- 红色禁止图标：对比项存在差异
- 橙色警告图标：对比项缺失
- 绿色正确图标：对比项内容完全一致

![差异类型](/wiki/images/9.jpg)

## 对比规则

### 列表对比规则

- 同级之间根据对象名称进行对比
- 如果找到名字对应的，作为一个对比组，进行属性对比
- 如果找不到名字对应的，则当前对比组为Miss类型（缺失左边或者右边）

对比前
```
Mine对象：          Their对象：
- Root              - Root
    - A                 - A
    - B                 - C
    - E                 - D       
    - F                 - E
                        - G
```

对比后：
```
Mine对象：          Their对象：
- Root              - Root
    - A                 - A
    - B                   
                        - C       
                        - D       
    - E                 - E
    - F                 
                        - G
```

### Component对比规则

- 相同Component类型的作为一个对比组
- 将对比组的Component转换成SerializedObject
- 获取其所有SerializedProperty
- 然后一一对SerializedProperty的值进行判断是否相等

### SerializedProperty对比规则

- SerializedPropertyType.Generic：忽略
- SerializedPropertyType.ObjectReference：
    - 判断是否都存在引用
    - 判断引用对象的GUID是否一样
    - 判断引用对象类型是否一致
    - 判断引用对象在Prefab中的路径是否一致（处理结构属性一致的两个Prefab，但GUID不一致的情况）
- 其他：使用SerializedProperty.DataEquals进行判断

## GUID和FileID变更问题

> 由于Prefab以及它的Component使用独有的GUID和FileID，因此对比时会有部分对比存在差异（即使两个Prefab一模一样）。

因此在`CompareUtility.cs`中加入`ingorePath`这个忽略列表，其中就忽略掉涉及GUID和FileID的字段。

**如果有误处理或者新增的忽略字段，可以在此修改。后续也会考虑增加Setting面板进行配置**。

