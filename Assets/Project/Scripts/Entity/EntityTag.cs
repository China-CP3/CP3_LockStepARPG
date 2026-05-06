using System;

[Flags]//Debug.Log(entity.Tags) 会打印出 3 而不是 Player | Enemy    Inspector 里不会显示成多选框（如果以后暴露到 Inspector）  Unity/IDE 的工具链识别不了这是位标志枚举
public enum EntityTag
{
    //总共32个
    None = 0,
    Player = 1<<0,
    Enemy = 1<<1,
    Item = 1<<2,

}
public partial class Entity
{
    public void AddTags(EntityTag tag)
    {
        this.tags = this.Tags | tag;//举例 1000 | 1001 = 1001
    }

    public void RemoveTags(EntityTag tag)
    {
        this.tags = this.Tags & ~tag;//举例 1000 & ~1001(0110) = 0000
    }

    public bool HasAllTag(EntityTag tag)
    {
        return (this.tags & tag) > 0;
    }

    public bool HasAnyTag(EntityTag tag)
    {
        return (this.tags & tag) > 0;
    }

    public void ClearTags()
    {
        this.tags = EntityTag.None;
    }
}