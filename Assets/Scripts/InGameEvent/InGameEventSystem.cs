using System;
using System.Collections.Generic;

public static class InGameEventSystem
{
    private static Dictionary<string, Type> eventNameTypeDict = new()
    {
        { "入侵", typeof(Invasion) },
        { "恒星耀斑", typeof(StellarFlare) },
        { "生物迁徙经过", typeof(BiologicalMigration) },
        { "出现裂缝", typeof(CracksAppear) },
        { "流星坠落", typeof(MeteorFall) },
        { "鼠患", typeof(RatInfestation) },
        { "灵光乍现", typeof(InspirationFlash) },
        { "呕吐", typeof(Vomit) },
    };

    private static List<InGameEvent> inGameEvents = new();

    static InGameEventSystem()
    {
        // 初始化代码
    }


}