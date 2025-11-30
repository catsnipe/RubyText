using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sample : MonoBehaviour
{
    [SerializeField]
    Button       Draw;

    [SerializeField]
    Button       Clear;

    [SerializeField]
    Button       All;

    [SerializeField]
    Slider       Position;

    [SerializeField]
    Slider       AutoForwardSpeed;

    [SerializeField]
    Toggle       IsDrawAtOnce;

    [SerializeField]
    RubyText     RubyText;

    // Start is called before the first frame update
    void Start()
    {
        RubyText.SetWH(900, 300);
        RubyText.SetFontSize(50);

        RubyText.SetText("この<color=red>{数:かず}</color>は、{一般的:いっぱんてき}に\r\n「{ナ:na}{ノ:no}」という\r\n{単位:たんい}{接頭辞:せっとうじ}を{使用:しよう}して{表:あらわ}されます。");

        // 表示位置の設定
        Position.onValueChanged.AddListener(
            (val) =>
            {
                RubyText.ForceTextPosition((int)((float)RubyText.GetTextLength() * val));
            }
        );

        // 表示速度
        AutoForwardSpeed.value = RubyText.AutoForwardSpeed;
        AutoForwardSpeed.onValueChanged.AddListener(
            (val) =>
            {
                RubyText.AutoForwardSpeed = val;
            }
        );

        // true... 文章を１度に表示
        // false...１文字ずつ表示
        IsDrawAtOnce.isOn = RubyText.IsDrawAtOnce;
        IsDrawAtOnce.onValueChanged.AddListener(
            (val) =>
            {
                RubyText.IsDrawAtOnce = val;
            }
        );

        // 表示開始
        Draw.onClick.AddListener(
            () =>
            {
                RubyText.SetText("この<color=red>{数:かず}</color>は、{一般的:いっぱんてき}に\r\n「{ナ:na}{ノ:no}」という\r\n{単位:たんい}{接頭辞:せっとうじ}を{使用:しよう}して{表:あらわ}されます。");
                RubyText.StartAutoForward();
            }
        );

        Clear.onClick.AddListener(
            () =>
            {
                RubyText.SetText("");
            }
        );

        All.onClick.AddListener(
            () =>
            {
                RubyText.ForceTextDrawAll();
            }
        );

    }
}
