using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AnimatorController : MonoBehaviour
{
    public Animator animator;
    public int UncomfortableState;
    public int HappinessState;
    public bool Tired1State;
    public bool Tired2State;
    public bool Hungry1State;
    public bool CloseToCamera1State;
    public bool CloseToCamera2State;

    public float mood;
    
    // 时间跟踪
    private float lastOneTimeAnimationTime = -999f;  // 上次播放一次性动画的时间
    private float lastCloseToCamera1Time = -999f;    // 上次播放贴近镜头_01的时间
    private float lastTiredAnimationTime = -999f;   // 上次播放困了动画的时间
    private float lastHungryAnimationTime = -999f;   // 上次播放饱食动画的时间
    
    // 检查间隔和概率
    private const float CHECK_INTERVAL = 3f;         // 每3秒检查一次
    private const float TRIGGER_PROBABILITY = 0.1f;  // 10%概率
    private const float TIRED_TRIGGER_PROBABILITY = 0.25f;  // 25%概率（困了动画）
    private const float HUNGRY_TRIGGER_PROBABILITY = 0.25f;  // 25%概率（饱食动画）
    private const float ONE_TIME_ANIMATION_COOLDOWN = 20f;  // 一次性动画冷却时间20秒
    private const float ONE_TIME_ANIMATION_COOLDOWN_TIRED = 10f;  // 困了动画的一次性动画冷却时间10秒
    private const float ONE_TIME_ANIMATION_COOLDOWN_HUNGRY = 10f;  // 饱食动画的一次性动画冷却时间10秒
    private const float CLOSE_TO_CAMERA_COOLDOWN = 120f;     // 贴近镜头_01冷却时间120秒
    private const float TIRED_ANIMATION_COOLDOWN = 120f;     // 困了动画冷却时间120秒
    private const float HUNGRY_ANIMATION_COOLDOWN = 120f;     // 饱食动画冷却时间120秒
    private const float MOOD_THRESHOLD = 0.5f;               // 心情阈值0.5
    private const float SOBRIETY_THRESHOLD = 50f;            // 清醒度阈值50
    private const float HUNGER_THRESHOLD = 50f;              // 饱食阈值50

    private void Start()
    {
        // 监听玩家状态更新事件
        EventManager.Instance.AddListener<Dictionary<PlayerStateEnum, State>>(EventType.RefreshAnimator, OnPlayerStateChanged);
        // 初始化时更新一次心情
        UpdateMood(StateManager.Instance.PlayerStateDict);
        
        // 启动检查协程
        StartCoroutine(CheckCloseToCamera1Coroutine());
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<Dictionary<PlayerStateEnum, State>>(EventType.RefreshAnimator, OnPlayerStateChanged);
    }

    /// <summary>
    /// 当玩家状态改变时更新心情
    /// </summary>
    private void OnPlayerStateChanged(Dictionary<PlayerStateEnum, State> playerStateDict)
    {
        UpdateMood(playerStateDict);
    }

    /// <summary>
    /// 更新心情值
    /// </summary>
    private void UpdateMood(Dictionary<PlayerStateEnum, State> playerStateDict)
    {
        if (animator == null || StateManager.Instance == null)
            return;

        // 获取五个状态的归一化值（当前值/最大值）
        float healthNormalized = GetNormalizedValue(playerStateDict, PlayerStateEnum.Health);
        float oxygenNormalized = GetNormalizedValue(playerStateDict, PlayerStateEnum.Oxygen);
        float hungerNormalized = GetNormalizedValue(playerStateDict, PlayerStateEnum.Hunger);
        float hydrationNormalized = GetNormalizedValue(playerStateDict, PlayerStateEnum.Hydration);
        float sanityNormalized = GetNormalizedValue(playerStateDict, PlayerStateEnum.Sanity);

        // 取最小值（最危险的状态）
        mood = Mathf.Min(
            healthNormalized,
            oxygenNormalized,
            hungerNormalized,
            hydrationNormalized,
            sanityNormalized
        );
        // 将心情值设置到Animator参数
        animator.SetFloat("Blend", mood);
    }

    /// <summary>
    /// 获取状态的归一化值（0-1之间）
    /// </summary>
    private float GetNormalizedValue(System.Collections.Generic.Dictionary<PlayerStateEnum, State> playerStateDict, PlayerStateEnum stateEnum)
    {
        if (!playerStateDict.ContainsKey(stateEnum))
            return 1f;

        var state = playerStateDict[stateEnum];
        float maxValue = state.MaxValue;
        
        // 避免除零
        if (maxValue <= 0)
            return 0f;

        // 归一化：当前值 / 最大值
        float normalized = state.CurValue / maxValue;
        
        // 确保值在0-1范围内
        return Mathf.Clamp01(normalized);
    }

    /// <summary>
    /// 标记播放了一次性动画（由外部调用）
    /// </summary>
    public void MarkOneTimeAnimationPlayed()
    {
        lastOneTimeAnimationTime = Time.time;
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    public void PlayAnimator(string TriggerName)
    {
        Debug.Log("播放动画：" + TriggerName);
        animator.SetTrigger(TriggerName);
        lastCloseToCamera1Time = Time.time;
    }

    /// <summary>
    /// 检查是否满足播放贴近镜头_01的条件
    /// </summary>
    private bool CanPlayCloseToCamera1()
    {
        float currentTime = Time.time;
        
        // 条件1：心情 >= 0.5
        if (mood < MOOD_THRESHOLD)
            return false;
        
        // 条件2：超过20秒没播一次性动画
        if (currentTime - lastOneTimeAnimationTime < ONE_TIME_ANIMATION_COOLDOWN)
            return false;
        
        // 条件3：超过180秒没播本动画
        if (currentTime - lastCloseToCamera1Time < CLOSE_TO_CAMERA_COOLDOWN)
            return false;
        
        return true;
    }

    /// <summary>
    /// 检查是否满足播放贴近镜头_02的条件
    /// </summary>
    private bool CanPlayCloseToCamera2()
    {
        float currentTime = Time.time;
        
        // 条件1：心情 >= 0.5
        if (mood < MOOD_THRESHOLD)
            return false;
        
        // 条件2：超过20秒没播一次性动画
        if (currentTime - lastOneTimeAnimationTime < ONE_TIME_ANIMATION_COOLDOWN)
            return false;
        
        // 条件3：超过120秒没播本动画
        if (currentTime - lastCloseToCamera1Time < CLOSE_TO_CAMERA_COOLDOWN)
            return false;
        
        return true;
    }

    /// <summary>
    /// 检查是否满足播放困了动画的条件
    /// </summary>
    private bool CanPlayTiredAnimation()
    {
        if (StateManager.Instance == null || GameManager.Instance == null)
            return false;
            
        float currentTime = Time.time;
        var playerStateDict = StateManager.Instance.PlayerStateDict;
        
        // 条件1：清醒度 <= 50
        if (!playerStateDict.ContainsKey(PlayerStateEnum.Sobriety))
            return false;
            
        float sobriety = playerStateDict[PlayerStateEnum.Sobriety].CurValue;
        if (sobriety > SOBRIETY_THRESHOLD)
            return false;
        
        // 条件2：超过10秒没播一次性动画
        if (currentTime - lastOneTimeAnimationTime < ONE_TIME_ANIMATION_COOLDOWN_TIRED)
            return false;
        
        // 条件3：超过120秒没播本动画
        if (currentTime - lastTiredAnimationTime < TIRED_ANIMATION_COOLDOWN)
            return false;
        
        return true;
    }

    /// <summary>
    /// 检查是否佩戴I1头部装备
    /// </summary>
    private bool HasHeadEquipment()
    {
            
        var headEquipment = GameManager.Instance.EquipmentBag.GetEquipmentByType(EquipmentType.Head);
        return headEquipment != null;
    }

    /// <summary>
    /// 检查是否满足播放饱食动画的条件
    /// </summary>
    private bool CanPlayHungryAnimation()
    {
        if (StateManager.Instance == null)
            return false;
            
        float currentTime = Time.time;
        var playerStateDict = StateManager.Instance.PlayerStateDict;
        
        // 条件1：饱食 <= 50
        if (!playerStateDict.ContainsKey(PlayerStateEnum.Hunger))
            return false;
            
        float hunger = playerStateDict[PlayerStateEnum.Hunger].CurValue;
        if (hunger > HUNGER_THRESHOLD)
            return false;
        
        // 条件2：超过10秒没播一次性动画
        if (currentTime - lastOneTimeAnimationTime < ONE_TIME_ANIMATION_COOLDOWN_HUNGRY)
            return false;
        
        // 条件3：超过120秒没播本动画
        if (currentTime - lastHungryAnimationTime < HUNGRY_ANIMATION_COOLDOWN)
            return false;
        
        return true;
    }

    /// <summary>
    /// 每3秒检查一次是否播放动画的协程
    /// </summary>
    private IEnumerator CheckCloseToCamera1Coroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(CHECK_INTERVAL);
            
            // 检查贴近镜头动画
            if (CanPlayCloseToCamera1())
            {
                // 10%概率播放
                if (Random.Range(0f, 1f) < TRIGGER_PROBABILITY)
                {
                    PlayAnimator("CloseToCamera1");
                }
            }
            else if (CanPlayCloseToCamera2())
            {
                // 10%概率播放
                if (Random.Range(0f, 1f) < TRIGGER_PROBABILITY)
                {
                    PlayAnimator("CloseToCamera2");
                }
            }
            // 检查困了动画
            else if (CanPlayTiredAnimation())
            {
                // 25%概率播放
                if (Random.Range(0f, 1f) < TIRED_TRIGGER_PROBABILITY)
                {
                    // 带I1头部装备就播困了_01，没带就播困了_02
                    if (HasHeadEquipment())
                    {
                        PlayAnimator("Tired1");
                    }
                    else
                    {
                        PlayAnimator("Tired2");
                    }
                }
            }
            // 检查饱食动画
            else if (CanPlayHungryAnimation())
            {
                // 25%概率播放
                if (Random.Range(0f, 1f) < HUNGRY_TRIGGER_PROBABILITY)
                {
                    PlayAnimator("Hungry1");
                }
            }
        }
    }
}
