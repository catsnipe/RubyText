// Copyright (c) catsnipe
// Released under the MIT license

// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the 
// "Software"), to deal in the Software without restriction, including 
// without limitation the rights to use, copy, modify, merge, publish, 
// distribute, sublicense, and/or sell copies of the Software, and to 
// permit persons to whom the Software is furnished to do so, subject to 
// the following conditions:
   
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software.
   
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#define TextMeshPro_Ver3_2_OR_LATER

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;

public class RubyText : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI         Text;

    [SerializeField]
    RectTransform           TextRect;

    [SerializeField]
    TextMeshProUGUI         Ruby;

    [SerializeField, Tooltip("１文字、または文章全体を表示する時間（秒）. ０の場合一度に表示"), Range(0, 1)]
    public float            AutoForwardSpeed;

    [SerializeField, Tooltip("true/１度に全文章を表示、false/１文字ずつ表示")]
    public bool             IsDrawAtOnce;

    [SerializeField, Tooltip("true/文字の表示ポジション固定、false/Alignment によっては文字表示ごとにポジションが変わる")]
    public bool             IsFixedPosition;

    /// <summary>
    /// 1文字描画終了
    /// </summary>
    public System.Action    OneWordDrawn;
    /// <summary>
    /// テキスト描画終了
    /// </summary>
    public System.Action    TextDrawFinished;

    class TextRuby
    {
        public int          TextPosition;
        public string       Word;
        public string       RubyWord;

        TextMeshProUGUI     parentText;
        TextMeshProUGUI     ruby;
        RectTransform       rubyRect;

        TMP_CharacterInfo[] characterInfos;
        int                 posTop;
        int                 posBtm;

        float               rubyWidth;

        /// <summary>
        /// .ctor
        /// </summary>
        public TextRuby(int no, TextMeshProUGUI rubyBase, TextMeshProUGUI _parentText)
        {
            parentText = _parentText;
            rubyWidth  = 0;

            ruby = Instantiate(rubyBase, parentText.transform);
            ruby.name = $"ruby {no}";

            rubyRect = ruby.GetComponent<RectTransform>();
            rubyRect.SetHeight(ruby.fontSize);
        }

        /// <summary>
        /// SetActive
        /// </summary>
        public void SetActive(bool active)
        {
            ruby.SetActive(active);
        }

        /// <summary>
        /// ルビワードの設定、表示範囲の設定
        /// </summary>
        public void Refresh()
        {
            if (RubyWord == null)
            {
                return;
            }

            var   top = characterInfos[posTop];
            var   btm = characterInfos[posBtm];

            float textWidth;
            
            textWidth = (btm.topRight.x - top.topLeft.x);

            ruby.SetActive(false);
            ruby.SetText(RubyWord);
            if (RubyWord.Length == 1)
            {
                // center
                ruby.alignment = TextAlignmentOptions.Bottom;
            }
            else
            {
                // 程よく間を空ける
                ruby.alignment = TextAlignmentOptions.BottomFlush;
            }

            rubyWidth = ruby.preferredWidth;
            if (Word.Length == RubyWord.Length && Word.Length == 2)
            {
                rubyRect.SetWidth(textWidth * 0.7f);
            }
            else
            if (rubyWidth < textWidth * 0.9f)
            {
                rubyRect.SetWidth(textWidth * 0.9f);
            }
            else
            {
                rubyRect.SetWidth(rubyWidth);
            }
            rubyRect.SetHeight(ruby.preferredHeight);
        }

        /// <summary>
        /// クリア
        /// </summary>
        public void Clear()
        {
            ruby.SetActive(false);
            ruby.SetText("");

            Word = null;
            RubyWord = null;
        }

        /// <summary>
        /// αの更新
        /// </summary>
        /// <param name="messagePosition">現在の最終文字表示位置</param>
        /// <param name="a">最終文字のα</param>
        public void UpdateAlpha(int messagePosition, float a)
        {
            if (posBtm > messagePosition)
            {
                ruby.color = new Color(ruby.color.r, ruby.color.g, ruby.color.b, 0);
            }
            else
            if (posBtm < messagePosition)
            {
                ruby.color = new Color(ruby.color.r, ruby.color.g, ruby.color.b, 1);
            }
            else
            {
                ruby.color = new Color(ruby.color.r, ruby.color.g, ruby.color.b, a);
            }
        }

        /// <summary>
        /// αの更新
        /// </summary>
        /// <param name="a">最終文字のα</param>
        public void UpdateAlpha(float a)
        {
            ruby.color = new Color(ruby.color.r, ruby.color.g, ruby.color.b, a);
        }

        /// <summary>
        /// ルビフォントサイズの設定
        /// </summary>
        /// <param name="fontSize">本文フォントのサイズ</param>
        public void SetFontSize(float fontSize)
        {
            ruby.fontSize    = 
            ruby.fontSizeMax = fontSize * 0.45f;
        }

        /// <summary>
        /// 計算済みの文字情報を設定
        /// </summary>
        /// <param name="characterInfos">計算済みの文字情報</param>
        /// <param name="posTop">ルビ先頭文字位置</param>
        /// <param name="posBtm">ルビ終了文字位置</param>
        /// <param name="rubyPositionAdjust">ルビの高さ補正値</param>
        public void SetTmpInfo(TMP_CharacterInfo[] characterInfos, int posTop, int posBtm, float rubyPositionAdjust)
        {
            this.characterInfos = characterInfos;
            this.posTop         = posTop;
            this.posBtm         = posBtm;

            var top = this.characterInfos[this.posTop];
            var btm = this.characterInfos[posBtm];

            float y = top.ascender + rubyPositionAdjust;
            float x = (top.topLeft.x + btm.topRight.x) / 2;

            rubyRect.SetXY(x, y);

            float r = (float)this.characterInfos[this.posBtm].color.r / 255;
            float g = (float)this.characterInfos[this.posBtm].color.g / 255;
            float b = (float)this.characterInfos[this.posBtm].color.b / 255;

            ruby.color = new Color(r, g, b, 0);
        }

        public void SetTmpInfo2(TMP_CharacterInfo[] characterInfos, int posTop, float rubyPositionAdjust)
        {
            this.characterInfos = characterInfos;
            this.posTop         = posTop;
            this.posBtm         = posTop;

            var top = this.characterInfos[posTop];
            var btm = this.characterInfos[posTop];

            float y = top.ascender + rubyPositionAdjust;
            float x = (top.topLeft.x + btm.topRight.x) / 2;

            rubyRect.SetXY(x, y);

            float r = (float)this.characterInfos[this.posBtm].color.r / 255;
            float g = (float)this.characterInfos[this.posBtm].color.g / 255;
            float b = (float)this.characterInfos[this.posBtm].color.b / 255;

            ruby.color = new Color(r, g, b, 0);
        }
    }

    /// <summary>
    /// 前回値. 現在値と比較し、違いがあったらそれぞれを更新
    /// </summary>
    class UpdateComparer
    {
        public string Message;
        public int    Position;
        public float  Alpha;
        public float  W, H;
        public bool   EnableWordWrapping;

        public UpdateComparer(TextMeshProUGUI text)
        {
            Clear(text);
        }

        public void Clear(TextMeshProUGUI text)
        {
            Message  = null;
            Position = 0;
            Alpha    = 0;
            W        = 0;
            H        = 0;
#if TextMeshPro_Ver3_2_OR_LATER
            EnableWordWrapping = text.textWrappingMode != TextWrappingModes.NoWrap;
#else
            EnableWordWrapping = text.enableWordWrapping == true;
#endif
        }
    }

    static Dictionary<string, float>
                                rubyAdjustByFont;

    List<TextRuby>              textRubys;
    int                         textRubyCount;
    UpdateComparer              updateComparer;
    TMP_CharacterInfo[]         cinfos;
    Regex                       searchAlpha = new Regex("<alpha=#[^>]+?>");


    float                       fontSizeMax;
    float                       fontSizeEx;

    string                      message;
    List<int>                   positionIndexes;

    int                         position;
    float                       alpha;
    int                         crlfCount;
    bool                        isBracketMessage;

    CoroutineInfo               coText;
    CoroutineInfo               coAuto;

    /// <summary>
    /// awake
    /// </summary>
    void Awake()
    {
        coText = new CoroutineInfo();
        coAuto = new CoroutineInfo();

        Ruby.SetActive(false);

        textRubys = new List<TextRuby>();
        updateComparer = new UpdateComparer(Text);

        fontSizeMax = Text.fontSizeMax;
        fontSizeEx  = 0;
    }

    /// <summary>
    /// update
    /// </summary>
    void Update()
    {
        bool update = false;

        if (updateComparer.W != TextRect.GetWidth() || updateComparer.H != TextRect.GetHeight())
        {
            updateComparer.W = TextRect.GetWidth();
            updateComparer.H = TextRect.GetHeight();
            update = true;
        }

#if TextMeshPro_Ver3_2_OR_LATER
        bool wrapping = Text.textWrappingMode != TextWrappingModes.NoWrap;
#else
        bool wrapping = Text.enableWordWrapping == true;
#endif

        if (updateComparer.EnableWordWrapping != wrapping)
        {
            updateComparer.EnableWordWrapping = wrapping;
            update = true;
        }

        if (update == true)
        {
            // 再描画
            updateComparer.Position = 0;
            updateComparer.Alpha = 0;

            this.StartSingleCoroutine(ref coText, textDrawing(false));
        }
    }

    void OnEnable()
    {
        this.ResumeSingleCoroutine(coAuto);
        this.ResumeSingleCoroutine(coText);
    }

    /// <summary>
    /// on destroy
    /// </summary>
    void OnDestroy()
    {
        this.StopSingleCoroutine(ref coAuto);
        this.StopSingleCoroutine(ref coText);
    }

    /// <summary>
    /// 指定されたフォントに対応したルビ補正位置（縦）を設定する
    /// </summary>
    public static void SetRubyPositionAdjust(TMP_FontAsset font, float rubyPositionAdjust)
    {
        if (rubyAdjustByFont == null)
        {
            rubyAdjustByFont = new Dictionary<string, float>();
        }

        if (rubyAdjustByFont.ContainsKey(font.name) == true)
        {
            rubyAdjustByFont[font.name] = rubyPositionAdjust;
        }
        else
        {
            rubyAdjustByFont.Add(font.name, rubyPositionAdjust);
        }
    }

    /// <summary>
    /// 文字自動送りの速度を設定する
    /// </summary>
    /// <param name="_secPerWord">ｎ秒で１文字表示</param>
    public void SetAutoForwardSpeed(float _secPerWord)
    {
        AutoForwardSpeed = _secPerWord;
    }

    /// <summary>
    /// 文字自動送り
    /// </summary>
    public void StartAutoForward()
    {
        if (positionIndexes == null)
        {
            return;
        }

        this.StartSingleCoroutine(ref coAuto, autoForward());
    }

    /// <summary>
    /// 文章の表示位置を強制する
    /// </summary>
    /// <param name="pos">整数：最終表示文字位置、少数：α</param>
    public void ForceTextPosition(float pos)
    {
        this.StopSingleCoroutine(ref coAuto);

        setTextPosition(pos);
    }

    /// <summary>
    /// 文字を全て表示する
    /// </summary>
    public void ForceTextDrawAll()
    {
        ForceTextPosition(GetTextLength());
    }

    /// <summary>
    /// テキストをクリア
    /// </summary>
    public void ResetText()
    {
        SetText(null);
    }

    /// <summary>
    /// 表示する文章の設定. {漢字:かんじ} の書式でルビを表現する
    /// </summary>
    /// <param name="_message">表示する文章</param>
    public void SetText(string _message)
    {
        if (_message == null)
        {
            _message = "";
        }

        message = _message;

        Text.SetText("");
        Text.fontSizeMax = fontSizeMax + fontSizeEx;

        textRubys.ForEach( ruby => ruby.Clear() );
        textRubyCount = 0;

        position = 0;
        alpha = 0;
        positionIndexes = null;
        isBracketMessage = false;

        var matches = Regex.Matches(_message, "\n");
        crlfCount = matches.Count;

        updateComparer.Clear(Text);

        if (string.IsNullOrEmpty(_message) == true)
        {
            return;
        }

        // <> ～ </> コマンドなし
        var notagMessage = Regex.Replace(message, "<[^<|>]+>", "");

        var cinfos = Text.GetTextInfo(Text.text).characterInfo;

        for ( ; ; )
        {
            int top = notagMessage.IndexOf("{");
            int btm = notagMessage.IndexOf("}");

            if (top < 0 || btm < 0)
            {
                break;
            }

            string   command = notagMessage.Substring(top, btm-top+1).Replace("{", "").Replace("}", "");

            try
            {
                // コマンド
                string[] coms    = command.Split(':');

                if (coms.Length == 2)
                {
                    if (textRubyCount+1 > textRubys.Count)
                    {
                        // なければバッファ拡張
                        textRubys.Add(new TextRuby(textRubyCount, Ruby, Text));
                    }

                    var ruby = textRubys[textRubyCount++];
                    //ruby.TextPosition = top;
                    ruby.Word         = coms[0];
                    ruby.RubyWord     = coms[1];

                    //Debug.Log($"{top} {string.Join(",", coms)}");

                }

                notagMessage = notagMessage.Remove(top, btm-top+1).Insert(top, coms[0]);
            }
            catch
            {
                Debug.LogError($"Unformat RubyWord: {command}");
                break;
            }
        }

        // {} コマンドなしの、TextMeshProUGUI に渡すテキスト
        message = Regex.Replace(message, ":[^\\}]+\\}", "").Replace("{", "");

        // ｍ文字目が文字列ｎ番目から表示されることを確認するリスト
        // （タグも加味するため ｍ≠ｎ）
        positionIndexes = new List<int>();
        for (int i = 0; i < message.Length; i++)
        {
            if (message[i] == '<')
            {
                i = message.IndexOf('>', i);
                continue;
            }
            positionIndexes.Add(i);
        }

        this.StartSingleCoroutine(ref coText, textDrawing(true));

        if (IsDrawAtOnce == true)
        {
            StartAutoForward();
        }
        //ienum_text = textDrawing(true);
        //this.StartSingleCoroutine(ref co_text, ienum_text);

//Debug.Log($"{positionIndexes.Count} {message}");
    }

    /// <summary>
    /// テキストの文字数を取得。タグは除く
    /// </summary>
    public int GetTextLength()
    {
        return positionIndexes == null ? 0 : positionIndexes.Count;
    }

    /// <summary>
    /// テキスト描画中は true、それ以外は false
    /// </summary>
    public bool CheckTextDrawing()
    {
        return coText.CoroutineExists() == true;
    }

    /// <summary>
    /// 話者の発言文字列描画中は true、それ以外は false
    /// </summary>
    public bool CheckTalking()
    {
        if (string.IsNullOrEmpty(message) == true || positionIndexes == null)
        {
            return false;
        }

        if (coText.CoroutineExists() == true)
        {
            char word = message[positionIndexes[position]];

            if (word == '（' || word == '(')
            {
                isBracketMessage = true;
            }
            else
            if (position < positionIndexes.Count-1 && (word == '）' || word == ')'))
            {
                isBracketMessage = false;
            }

            switch (word)
            {
                case '「':
                case '」':
                case '（':
                case '）':
                case '…':
                case '、':
                case '。':
                case '！':
                case '？':
                case '!':
                case '?':
                case ',':
                case '.':
                case '(':
                case ')':
                    return false;
                default:
                    return true;
            }

        }

        return false;
    }

    /// <summary>
    /// （）で囲まれたメッセージ表示中なら true
    /// </summary>
    /// <returns></returns>
    public bool CheckBracketStart()
    {
        if (message.Length > 0)
        {
            char word = message[0];

            switch (word)
            {
                case '（':
                case '(':
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// （）で囲まれたメッセージ表示中なら true
    /// </summary>
    /// <returns></returns>
    public bool CheckBracketMessage()
    {
        return isBracketMessage;
    }

    /// <summary>
    /// RectXY
    /// </summary>
    public void SetXY(float x, float y)
    {
        SetX(x);
        SetY(y);
    }

    /// <summary>
    /// RectX
    /// </summary>
    public void SetX(float x)
    {
        TextRect.SetX(x);
    }

    public float GetX()
    {
        return TextRect.GetX();
    }
    
    /// <summary>
    /// RectY
    /// </summary>
    public void SetY(float y)
    {
        TextRect.SetY(y);
    }

    public float GetY()
    {
        return TextRect.GetY();
    }
    
    /// <summary>
    /// RectWH
    /// </summary>
    public void SetWH(float width, float height)
    {
        SetWidth(width);
        SetHeight(height);
    }

    /// <summary>
    /// RectW
    /// </summary>
    public void SetWidth(float width)
    {
        TextRect.SetWidth(width);
        updateComparer.W = width;
    }

    public float GetWidth()
    {
        return TextRect.GetWidth();
    }
    
    /// <summary>
    /// RectH
    /// </summary>
    public void SetHeight(float height)
    {
        TextRect.SetHeight(height);
        updateComparer.H = height;
    }

    public float GetHeight()
    {
        return TextRect.GetHeight();
    }

    /// <summary>
    /// FontSize
    /// </summary>
    /// <returns></returns>
    public float GetFontSizeMax()
    {
        return fontSizeMax;
    }

    public float GetFontSizeEx()
    {
        return fontSizeEx;
    }

    /// <summary>
    /// フォント設定
    /// </summary>
    public void SetFont(TMP_FontAsset font, Material fontMaterial)
    {
        Text.font = font;
        Text.fontMaterial = fontMaterial;
        Ruby.font = font;
        Ruby.fontMaterial = fontMaterial;
    }

    /// <summary>
    /// フォントの AutoSize を設定する
    /// </summary>
    public void SetFontAutoSize(float min, float max)
    {
        Text.enableAutoSizing = true;
        Text.fontSizeMin = min;
        Text.fontSizeMax = max + fontSizeEx;
        Text.fontSize    = max + fontSizeEx;
        fontSizeMax      = max;
    }

    /// <summary>
    /// フォントの AutoSize 機能をリセットする
    /// </summary>
    public void ResetFontAutoSize()
    {
        Text.enableAutoSizing = false;
    }
    
    /// <summary>
    /// フォントサイズを設定する
    /// </summary>
    public void SetFontSize(float size)
    {
        Text.fontSizeMax = size + fontSizeEx;
        Text.fontSize    = size + fontSizeEx;
        fontSizeMax      = size;
    }

    /// <summary>
    /// フォントサイズ（追加サイズ）を設定する
    /// </summary>
    public void SetFontSizeEx(float size)
    {
        Text.fontSizeMax = fontSizeMax + size;
        Text.fontSize    = fontSizeMax + size;
        fontSizeEx       = size;
    }

    /// <summary>
    /// カラーを設定する
    /// </summary>
    public void SetColor(Color color)
    {
        Text.color = color;
    }

    /// <summary>
    /// 文字寄せのタイプを設定
    /// </summary>
    public void SetAlignment(TextAlignmentOptions alignment)
    {
        Text.alignment = alignment;
    }

    /// <summary>
    /// 文字寄せのタイプを設定
    /// </summary>
    public void SetFixedPosition(bool isFixed)
    {
        IsFixedPosition = isFixed;
    }

    /// <summary>
    /// 文字間を設定
    /// </summary>
    public void SetCharacterSpacing(float spacing)
    {
        Text.characterSpacing = spacing;
    }

    /// <summary>
    /// 行間を設定
    /// </summary>
    public void SetLineSpacing(float spacing)
    {
        Text.lineSpacing = spacing;
    }

#if TextMeshPro_Ver3_2_OR_LATER
    /// <summary>
    /// 禁則モードを設定
    /// </summary>
    public void SetTextWrappingMode(TextWrappingModes mode)
    {
        Text.textWrappingMode = mode;
    }
#endif

    /// <summary>
    /// レイキャストの On / Off
    /// </summary>
    public void SetRaycastTarget(bool targetOn)
    {
        Text.raycastTarget = targetOn;
    }

    /// <summary>
    /// TextMeshProUGUI を返す
    /// </summary>
    public TextMeshProUGUI GetUIText()
    {
        return Text;
    }

    /// <summary>
    /// 文字自動送り
    /// </summary>
    /// <returns></returns>
    IEnumerator autoForward()
    {
        int   max = positionIndexes.Count;
        float time = 0;

        if (AutoForwardSpeed == 0)
        {
            setTextPosition(max);
            yield break;
        }

        yield return null;

        for ( ; time < max; )
        {
            time += Time.deltaTime * (1.0f / AutoForwardSpeed);
            
            setTextPosition(time);

            yield return null;
        }

        coAuto.Clear();
    }

    /// <summary>
    /// テキスト描画
    /// </summary>
    IEnumerator textDrawing(bool calculate)
    {
        if (calculate == true)
        {
            Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, 0);

            // 一旦テキストが見えない状態で全テキストを描画する（文字情報を取得するため）
            Text.SetText(message);
        }

        // １フレーム経過しないと文字情報が更新されない
        yield return null;

        // 全文字が表示された状態のフォントサイズがフォント最大サイズとする
        // （これをしておかないと、文字が少ない時バカでかい文字になるなど、不安定なテキスト描画になる）
        Text.fontSizeMax = fontSizeMax + fontSizeEx;

        // 出そろった文字情報を元にルビを設定
        refreshRuby();

        Text.SetText("");

        // カラーを戻す
        Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, 1);

        while (true)
        {
            var  msg   = Text.text;
            bool isEnd = false;

//DDisp.Log($"{position} {alpha}");
            // 文章が変わったり、表示位置が変化
            if (updateComparer.Message != message || updateComparer.Position != position)
            {
                if (positionIndexes != null && position >= 0 && position < positionIndexes.Count)
                {
                    int    ia   = (int)(255 * alpha);
                    string taga = $"<alpha=#{ia.ToString("x2")}>";

                    if (IsDrawAtOnce == true)
                    {
                        msg = taga + message;
                        msg = Regex.Replace(msg, $"(?<tag><color=[^>]+>)", "${tag}" + taga);
                        msg = msg.Replace("</color>", "</color>" + taga);
                        refreshRubyAlpha(alpha);
                    }
                    else
                    {
                        msg = message.Substring(0, positionIndexes[position]+1);

                        var msgAfter = message.Remove(0, positionIndexes[position]+1);

                        if (msg.Length >= 1)
                        {
                            msg = msg.Insert(positionIndexes[position], taga);

                            refreshRubyAlpha(alpha);
                        }

                        if (IsFixedPosition == true)
                        {
                            msg += "<alpha=#00>" + msgAfter;
                        }
                        else
                        {
                            // 行が増えるごとにテキストの表示位置（縦）が変わってしまうのを防ぐ
                            var matches = Regex.Matches(msg, "\n");
                            for (int i = 0; i < crlfCount - matches.Count; i++)
                            {
                                msg += "\n";
                            }
                        }

                        if (msg[msg.Length-1] == '\n')
                        {
                            msg += "　";
                        }
                    }

                    OneWordDrawn?.Invoke();

//Debug.Log($"1: {position} {alpha} {msg}");
                }

                updateComparer.Message = message;
                updateComparer.Position = position;
                updateComparer.Alpha = alpha;
            }
            else
            // αだけ変化
            if (updateComparer.Alpha != alpha)
            {
                if (msg.Length > 1)
                {
                    if (alpha == 1 && this.position == GetTextLength()-1)
                    {
                        msg = searchAlpha.Replace(msg, "");
                        isEnd = true;
                    }
                    else
                    {
                        int ia = (int)(255 * alpha);
                        msg = searchAlpha.Replace(msg, $"<alpha=#{ia.ToString("x2")}>");
                    }

                    refreshRubyAlpha(alpha);
                }

                updateComparer.Alpha = alpha;
//Debug.Log($"2: {position} {alpha} {msg}");
            }

            if (Text.text != msg)
            {
                Text.SetText(msg);
            }

            if (isEnd == true)
            {
                break;
            }

            yield return null;
        }

        yield return null;

        coText.Clear();

        TextDrawFinished?.Invoke();
    }

    /// <summary>
    /// ルビの再設定
    /// </summary>
    void refreshRuby()
    {
        for (int i = 0; i < textRubys.Count; i++)
        {
            textRubys[i].SetActive(false);
        }

        // 文字描画後の文字情報（表示位置やカラーなど）
        cinfos = Text.GetTextInfo(Text.text).characterInfo;

        int posTop = 0;
        int posBtm = 0;

        for (int i = 0; i < textRubys.Count; i++)
        {
            var ruby    = textRubys[i];
            if (ruby.Word == null)
            {
                continue;
            }

            float adjust = 0;
            if (rubyAdjustByFont != null && rubyAdjustByFont.ContainsKey(Text.font.name) == true)
            {
                adjust = rubyAdjustByFont[Text.font.name];
            }

            int no = 0;

            for (int j = posBtm; j < cinfos.Length; j++)
            {
                if (cinfos[j].character == ruby.Word[no])
                {
                    if (no == 0)
                    {
                        posTop = j;
                    }
                    if (++no >= ruby.Word.Length)
                    {
                        posBtm = j;
                        break;
                    }
                }
                else
                {
                    no = 0;
                }
            }

            // ルビを振る文字列のうち、同じ高さの終端文字を検索する
            // （文字列が自動改行などで２行にまたがってしまう問題の対策）
            for (int j = posTop; j <= posBtm; j++)
            {
                if (cinfos[j].character == '\r' ||
                    cinfos[j].character == '\n' )
                {
                    posBtm = j-1;
                    break;
                }
            }

            if (ruby.Word.Length == ruby.RubyWord.Length)
            {
//                ruby.SetTmpInfo2(cinfos, posTop+i, adjust);
                ruby.SetTmpInfo(cinfos, posTop, posBtm, adjust);
            }
            else
            {
                ruby.SetTmpInfo(cinfos, posTop, posBtm, adjust);
            }
            //ruby.SetFontSize(fontSizeMax + fontSizeEx);
            ruby.SetFontSize(fontSizeMax);
            ruby.Refresh();
            ruby.SetActive(true);
        }

        refreshRubyAlpha(0);
    }

    /// <summary>
    /// ルビの表示（α更新）
    /// </summary>
    void refreshRubyAlpha(float alpha)
    {
        if (IsDrawAtOnce == true)
        {
            textRubys.ForEach( ruby => ruby.UpdateAlpha(alpha) );
        }
        else
        {
            textRubys.ForEach( ruby => ruby.UpdateAlpha(position, alpha) );
        }
    }

    /// <summary>
    /// 文字の表示位置設定
    /// </summary>
    /// <param name="pos">整数：最終表示文字位置、少数：α</param>
    void setTextPosition(float pos)
    {
        int count = GetTextLength();

        if (IsDrawAtOnce == true)
        {
            this.position = count - 1;
            alpha = pos;
            if (alpha > 1)
            {
                alpha = 1;
            }
        }
        else
        {
            if (pos < 0)
            {
                this.position = 0;
                alpha         = 0;
            }
            else
            if (pos >= count)
            {
                this.position = count - 1;
                alpha         = 1;
            }
            else
            {
                this.position = (int)pos;
                alpha         = pos - this.position;
            }
        }
    }

}
