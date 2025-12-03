using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace ThreeKingdoms.UI
{
    /// <summary>
    /// TextMeshPro字体辅助工具类
    /// 提供字体加载、设置、管理功能
    /// </summary>
    public static class TMPFontHelper
    {
        // 字体缓存
        private static TMP_FontAsset defaultFont;
        private static TMP_FontAsset chineseFont;
        private static TMP_FontAsset koreanFont;
        private static TMP_FontAsset englishFont;

        /// <summary>
        /// 获取默认字体
        /// </summary>
        public static TMP_FontAsset GetDefaultFont()
        {
            if (defaultFont == null)
            {
                // 尝试从Resources加载
                defaultFont = Resources.Load<TMP_FontAsset>("Fonts/DefaultFont SDF");

                // 如果没有自定义字体，使用TMP默认字体
                if (defaultFont == null)
                {
                    defaultFont = TMP_Settings.defaultFontAsset;
                    Debug.LogWarning("[TMPFontHelper] 使用TMP默认字体");
                }
            }

            return defaultFont;
        }

        /// <summary>
        /// 获取中文字体
        /// </summary>
        public static TMP_FontAsset GetChineseFont()
        {
            if (chineseFont == null)
            {
                // ⭐ 用户字体路径：Assets/TextMesh Pro/Fonts/MSYH SDF_1.asset
                // Resources路径：TextMesh Pro/Fonts/MSYH SDF_1
                chineseFont = Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/MSYH SDF_1");

                if (chineseFont == null)
                {
                    Debug.LogWarning("[TMPFontHelper] 找不到中文字体 (TextMesh Pro/Fonts/MSYH SDF_1)，使用默认字体");
                    chineseFont = GetDefaultFont();
                }
                else
                {
                    Debug.Log("[TMPFontHelper] 成功加载中文字体: MSYH SDF_1");
                }
            }

            return chineseFont;
        }

        /// <summary>
        /// 获取韩文字体
        /// </summary>
        public static TMP_FontAsset GetKoreanFont()
        {
            if (koreanFont == null)
            {
                // ⭐ 用户字体路径：Assets/TextMesh Pro/Fonts/KoreanFont SDF.asset
                // Resources路径：TextMesh Pro/Fonts/KoreanFont SDF
                koreanFont = Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/KoreanFont SDF");

                if (koreanFont == null)
                {
                    Debug.LogWarning("[TMPFontHelper] 找不到韩文字体 (TextMesh Pro/Fonts/KoreanFont SDF)，使用默认字体");
                    koreanFont = GetDefaultFont();
                }
                else
                {
                    Debug.Log("[TMPFontHelper] 成功加载韩文字体: KoreanFont SDF");
                }
            }

            return koreanFont;
        }

        /// <summary>
        /// 获取英文字体
        /// </summary>
        public static TMP_FontAsset GetEnglishFont()
        {
            if (englishFont == null)
            {
                // 尝试加载英文字体
                englishFont = Resources.Load<TMP_FontAsset>("Fonts/EnglishFont SDF");

                if (englishFont == null)
                {
                    // 英文可以使用默认字体
                    englishFont = GetDefaultFont();
                }
            }

            return englishFont;
        }

        /// <summary>
        /// 根据语言获取对应字体
        /// </summary>
        public static TMP_FontAsset GetFontForLanguage(ThreeKingdoms.Language language)
        {
            switch (language)
            {
                case ThreeKingdoms.Language.Chinese:
                    return GetChineseFont();

                case ThreeKingdoms.Language.Korean:
                    return GetKoreanFont();

                case ThreeKingdoms.Language.English:
                    return GetEnglishFont();

                default:
                    return GetDefaultFont();
            }
        }

        /// <summary>
        /// 设置TextMeshProUGUI组件的字体
        /// </summary>
        public static void SetFont(TextMeshProUGUI tmpComponent, TMP_FontAsset font)
        {
            if (tmpComponent != null && font != null)
            {
                tmpComponent.font = font;
            }
        }

        /// <summary>
        /// 根据当前语言设置字体
        /// </summary>
        public static void SetFontByLanguage(TextMeshProUGUI tmpComponent)
        {
            if (tmpComponent == null) return;

            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                ThreeKingdoms.Language currentLanguage = ThreeKingdoms.LocalizationManager.Instance.GetCurrentLanguage();
                TMP_FontAsset font = GetFontForLanguage(currentLanguage);
                SetFont(tmpComponent, font);
            }
            else
            {
                // 如果没有本地化系统，使用默认字体
                SetFont(tmpComponent, GetDefaultFont());
            }
        }

        /// <summary>
        /// 为GameObject下所有TMP组件设置字体
        /// </summary>
        public static void SetFontForAllTMP(GameObject root, TMP_FontAsset font)
        {
            if (root == null || font == null) return;

            // 获取所有TextMeshProUGUI组件（包括子对象）
            TextMeshProUGUI[] tmpComponents = root.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (var tmp in tmpComponents)
            {
                SetFont(tmp, font);
            }

            Debug.Log($"[TMPFontHelper] 为 {root.name} 下的 {tmpComponents.Length} 个TMP组件设置了字体");
        }

        /// <summary>
        /// 根据当前语言为GameObject下所有TMP组件设置字体
        /// </summary>
        public static void SetFontForAllTMPByLanguage(GameObject root)
        {
            if (root == null) return;

            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                ThreeKingdoms.Language currentLanguage = ThreeKingdoms.LocalizationManager.Instance.GetCurrentLanguage();
                TMP_FontAsset font = GetFontForLanguage(currentLanguage);
                SetFontForAllTMP(root, font);
            }
            else
            {
                SetFontForAllTMP(root, GetDefaultFont());
            }
        }

        /// <summary>
        /// 清除字体缓存（用于重新加载字体）
        /// </summary>
        public static void ClearFontCache()
        {
            defaultFont = null;
            chineseFont = null;
            koreanFont = null;
            englishFont = null;

            Debug.Log("[TMPFontHelper] 字体缓存已清除");
        }

        /// <summary>
        /// 检查字体是否存在
        /// </summary>
        public static bool IsFontAvailable(ThreeKingdoms.Language language)
        {
            TMP_FontAsset font = null;

            switch (language)
            {
                case ThreeKingdoms.Language.Chinese:
                    font = Resources.Load<TMP_FontAsset>("Fonts/ChineseFont SDF");
                    break;

                case ThreeKingdoms.Language.Korean:
                    font = Resources.Load<TMP_FontAsset>("Fonts/KoreanFont SDF");
                    break;

                case ThreeKingdoms.Language.English:
                    font = Resources.Load<TMP_FontAsset>("Fonts/EnglishFont SDF");
                    break;
            }

            return font != null;
        }

        /// <summary>
        /// 打印字体加载状态（用于调试）
        /// </summary>
        public static void PrintFontStatus()
        {
            Debug.Log("===== TMP字体状态 =====");
            Debug.Log($"默认字体: {(GetDefaultFont() != null ? "✓" : "✗")}");
            Debug.Log($"中文字体: {(IsFontAvailable(ThreeKingdoms.Language.Chinese) ? "✓" : "✗")}");
            Debug.Log($"韩文字体: {(IsFontAvailable(ThreeKingdoms.Language.Korean) ? "✓" : "✗")}");
            Debug.Log($"英文字体: {(IsFontAvailable(ThreeKingdoms.Language.English) ? "✓" : "✗")}");
            Debug.Log("======================");
        }

        /// <summary>
        /// 创建带字体的TextMeshProUGUI组件
        /// </summary>
        public static TextMeshProUGUI CreateTMPWithFont(GameObject parent, string name, ThreeKingdoms.Language language)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();

            // 设置字体
            TMP_FontAsset font = GetFontForLanguage(language);
            SetFont(tmp, font);

            // 设置基本属性
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            // 设置RectTransform
            RectTransform rt = tmp.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            return tmp;
        }

        /// <summary>
        /// 设置字体材质（用于特殊效果）
        /// </summary>
        public static void SetFontMaterial(TextMeshProUGUI tmpComponent, Material material)
        {
            if (tmpComponent != null && material != null)
            {
                tmpComponent.fontMaterial = material;
            }
        }

        /// <summary>
        /// 重置字体材质为默认
        /// </summary>
        public static void ResetFontMaterial(TextMeshProUGUI tmpComponent)
        {
            if (tmpComponent != null && tmpComponent.font != null)
            {
                tmpComponent.fontMaterial = tmpComponent.font.material;
            }
        }
    }
}