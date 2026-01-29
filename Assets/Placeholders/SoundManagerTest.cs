using UnityEngine;
using UnityEngine.InputSystem;

public class SoundManagerTest : MonoBehaviour
{
    private Keyboard kb;

    void Awake()
    {
        kb = Keyboard.current;
    }

    void Update()
    {
        kb ??= Keyboard.current;
        if (kb == null) return;

        // 1: 播放一次（非循环）
        if (kb.digit1Key.wasPressedThisFrame)
            SoundManager.Instance.PlaySound(301, false);

        // 2: 循环播放
        if (kb.digit2Key.wasPressedThisFrame)
            SoundManager.Instance.PlaySound(302, true);

        // 3: 停止 301
        if (kb.digit3Key.wasPressedThisFrame)
            SoundManager.Instance.StopSound(301);

        // 4: 暂停 302
        if (kb.digit4Key.wasPressedThisFrame)
            SoundManager.Instance.PauseSound(302);

        // 5: 继续 302
        if (kb.digit5Key.wasPressedThisFrame)
            SoundManager.Instance.UnpauseSound(302);

        // R: 重播同一个 id（默认策略）
        if (kb.rKey.wasPressedThisFrame)
            SoundManager.Instance.PlaySound(301, false, restartIfAlreadyPlaying: true);

        // T: 同 id 正在播时不做事
        if (kb.tKey.wasPressedThisFrame)
            SoundManager.Instance.PlaySound(301, false, restartIfAlreadyPlaying: false);
    }
}
