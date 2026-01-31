using UnityEngine;
using UnityEngine.UI;

public class SteveA : MonoBehaviour
{
    [Header("Refs")]
    public AnimalController cat;          // 需要有 cat.right()
    public BubbleAppear bubble;              // 需要有 bubble.Appear()
    public SpriteRenderer tvRenderer;  // TV 的 SpriteRenderer

    [Header("Option UI")]
    public GameObject optionPanel;     // 按钮容器，默认隐藏
    public Button tvButton;
    public Button musicButton;

    [Header("TV Sprites")]
    public Sprite tvChangedSprite;

    [Header("Sound IDs")]
    public int bootSfxId = 301;
    public int wrongSfxId = 302;
    public int tvSfxId = 303;
    public int tvAfterChangeSfxId = 304; // 可选：TV换图后再播一次
    public int musicIntroSfxId = 305;    // Music() 先播一次（非循环），播完进入 loop
    public int musicLoopSfxId = 306;     // MusicSFXFinished() 开始循环播（可与 intro 相同也行）

    private int seq = 0;

    void Awake()
    {
        if (optionPanel != null) optionPanel.SetActive(false);

        if (tvButton != null)
        {
            tvButton.onClick.RemoveListener(TV);
            tvButton.onClick.AddListener(TV);
        }

        if (musicButton != null)
        {
            musicButton.onClick.RemoveListener(Music);
            musicButton.onClick.AddListener(Music);
        }
    }

    // 正确结果（由 MaskInteract 调用）
    public void Right()
    {
        int mySeq = ++seq;
        if (optionPanel != null) optionPanel.SetActive(false);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(
                bootSfxId,
                false,
                onCompleted: () =>
                {
                    if (seq != mySeq) return;
                    BootSFXFinished();
                }
            );
        }
    }

    // 错误结果（由 MaskInteract 调用）
    public void Wrong()
    {
        SoundManager.Instance?.PlaySound(wrongSfxId, false);

        if (bubble != null)
            bubble.Appear();
    }

    // boot 播完：出现按钮
    public void BootSFXFinished()
    {
        if (optionPanel != null)
            optionPanel.SetActive(true);
    }

    // 选项：TV
    public void TV()
    {
        int mySeq = ++seq;
        if (optionPanel != null) optionPanel.SetActive(false);

        SoundManager.Instance?.PlaySound(
            tvSfxId,
            false,
            onCompleted: () =>
            {
                if (seq != mySeq) return;
                TVSFXFinished();
            }
        );
    }

    // 选项：Music
    public void Music()
    {
        int mySeq = ++seq;
        if (optionPanel != null) optionPanel.SetActive(false);

        SoundManager.Instance?.PlaySound(
            musicIntroSfxId,
            false,
            onCompleted: () =>
            {
                if (seq != mySeq) return;
                MusicSFXFinished();
            }
        );
    }

    // TV 音效结束
    public void TVSFXFinished()
    {
        if (tvRenderer != null && tvChangedSprite != null)
            tvRenderer.sprite = tvChangedSprite;

        if (tvAfterChangeSfxId != 0)
            SoundManager.Instance?.PlaySound(tvAfterChangeSfxId, false);

        if (cat != null)
            cat.Right(); // 猫跑走
    }

    // Music 音效结束
    public void MusicSFXFinished()
    {
        // 开始循环播音乐
        int loopId = (musicLoopSfxId != 0) ? musicLoopSfxId : musicIntroSfxId;
        SoundManager.Instance?.PlaySound(loopId, true);

        // 完成任务
        if (TaskList.Instance != null)
            TaskList.Instance.CompleteTask(400);
    }
}
