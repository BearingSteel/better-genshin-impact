using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace BetterGenshinImpact.GameTask.BearingSteel;





/// <summary>
/// 自动战斗配置
/// </summary>
[Serializable]
public partial class BearingSteelConfig : ObservableObject
{

    
    [ObservableProperty] 
    private bool _bearingSteelConfigEnable = true ;
    
    [ObservableProperty] 
    private bool _bearingSteelAutoSkill = true;
    
    [ObservableProperty] 
    private bool _bearingSteelAutoEatEgg = true;
    
    [ObservableProperty] 
    private bool _bearingSteelReduceWait = true;
    
    [ObservableProperty]
    private bool _bearingSteelCheckElitePickUp = true;
    
    [ObservableProperty]
    private bool _bearingSteelCheckAfterSwitch = true;
    
    [ObservableProperty]
    private bool _bearingSteelPartySwitchWhileInject = true;


    public static bool GetBearingSteelConfigEnable()
    {
        return TaskContext.Instance().Config.BearingSteelConfig.BearingSteelConfigEnable;
    }

    public static bool GetBearingSteelAutoSkill(string path)
    {
        if (path.ToUpper().EndsWith("\\自动EQ.TXT"))
            return true;
        return TaskContext.Instance().Config.BearingSteelConfig.BearingSteelConfigEnable
        && TaskContext.Instance().Config.BearingSteelConfig.BearingSteelAutoSkill;
    }

    public static bool GetBearingSteelAutoSkill()
    {
        return TaskContext.Instance().Config.BearingSteelConfig.BearingSteelConfigEnable
        && TaskContext.Instance().Config.BearingSteelConfig.BearingSteelAutoSkill;
    }
    public static bool GetBearingSteelAutoEatEgg()
    {
        return TaskContext.Instance().Config.BearingSteelConfig.BearingSteelConfigEnable
        && TaskContext.Instance().Config.BearingSteelConfig.BearingSteelAutoEatEgg;
    }
    public static bool GetBearingSteelReduceWait()
    {
        return TaskContext.Instance().Config.BearingSteelConfig.BearingSteelConfigEnable
        && TaskContext.Instance().Config.BearingSteelConfig.BearingSteelReduceWait;
    }
    public static bool GetBearingSteelCheckElitePickUp()
    {
        return TaskContext.Instance().Config.BearingSteelConfig.BearingSteelConfigEnable
        && TaskContext.Instance().Config.BearingSteelConfig.BearingSteelCheckElitePickUp;
    }
    public static bool GetBearingSteelCheckAfterSwitch()
    {
        return TaskContext.Instance().Config.BearingSteelConfig.BearingSteelConfigEnable
        && TaskContext.Instance().Config.BearingSteelConfig.BearingSteelCheckAfterSwitch;
    }
    public static bool GetBearingSteelPartySwitchWhileInject()
    {
        return TaskContext.Instance().Config.BearingSteelConfig.BearingSteelConfigEnable
        && TaskContext.Instance().Config.BearingSteelConfig.BearingSteelPartySwitchWhileInject;
    }


}
