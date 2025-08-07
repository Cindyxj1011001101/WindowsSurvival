namespace ChatPlugIn
{
    public enum NodeType
    {
        //基础
        Base =0,
        //零进零出
        ZeroInZeroOut =1,
        //零进单出
        ZeroInSingleOut = 2,
        //零进多出
        ZeroInMulti0ut =3,
        //单进零出
        SingleInZero0ut =4,
        //单进单出
        SingleInSingleOut =5,
        //单进多出
        SingleInMulti0ut =6,
        //多进零出
        MultiInZeroOut =7,
        //多进单出
        MultiInSingleOut =8,
        //多进多出
        MultiInMulti0ut =9,   
        
        
        //开始对话触发条件
        //开始对话优先级
        //进入该条对话条件
        //进入各对话分支判断条件
        //对话延迟时间
        //对话结束效果
        
        //开始
        Start=21,
        //结束
        End=41,
        //对话
        Dialogue=51,
        //分支
        BranchCondition=61,
        //通过条件
        PassCondition=71,
        
    }
}