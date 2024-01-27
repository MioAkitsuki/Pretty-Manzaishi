using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Kuchinashi.SceneControl
{
    public partial class SceneControl : MonoBehaviour
    {
        public static string CurrentScene;

        public static SceneControl Instance;
        private CanvasGroup mCanvasGroup;
        private Slider mProgress;

        private Animator mAnimator;
        private TMP_Text mLabel;

        public static bool CanTransition;
        public static bool IsTransiting;

        private void Awake()
        {
            Instance = this;
            CurrentScene = "ControlScene";

            mCanvasGroup = GetComponent<CanvasGroup>();
            mProgress = GetComponentInChildren<Slider>();

            mAnimator = GetComponent<Animator>();
            mLabel = transform.Find("Label").GetComponent<TMP_Text>();

            LoadSceneWithoutConfirm("StartScene");
        }
    }

    public partial class SceneControl
    {
        public static void LoadScene(string targetScene, Action action = null)
        {
            if (targetScene == CurrentScene) return;

            CanTransition = false;
            Instance.StartCoroutine(Instance.LoadSceneCoroutine(targetScene, action));
        }
        
        public static void LoadSceneWithoutConfirm(string targetScene, Action action = null)
        {
            if (targetScene == CurrentScene) return;

            CanTransition = true;
            Instance.StartCoroutine(Instance.LoadSceneCoroutine(targetScene, action));
        }

        public static void ReloadSceneWithoutConfirm(string targetScene, Action action = null)
        {
            if (CurrentScene == "ControlScene") return;

            CanTransition = true;
            Instance.StartCoroutine(Instance.ReloadSceneCoroutine(targetScene, action));
        }

        public static void UnloadScene(string[] targetScene)
        {
            if (CurrentScene == "ControlScene") return;

            Instance.StartCoroutine(Instance.UnloadSceneCoroutine(targetScene));
        }

        public static void SwitchScene(string targetScene, Action action = null)
        {
            if (targetScene == CurrentScene) return;

            CanTransition = false;
            Instance.StartCoroutine(Instance.SwitchSceneCoroutine(targetScene, action));
        }

        public static void SwitchSceneWithoutConfirm(string targetScene, Action action = null)
        {
            if (targetScene == CurrentScene) return;

            CanTransition = true;
            Instance.StartCoroutine(Instance.SwitchSceneCoroutine(targetScene, action));
        }

        IEnumerator LoadSceneCoroutine(string targetScene, Action action = null)
        {
            yield return Fade(1);
            yield return new WaitForSeconds(1f);

            IsTransiting = true;

            var operation = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
            operation.allowSceneActivation = false;

            while (operation.progress <= 0.9f)
            {
                if (operation.progress == 0.9f && mProgress.value == 0.9f)
                {
                    break;
                }

                mProgress.value = (mProgress.value + (operation.progress - mProgress.value) / 10) > 0.85f ? 0.9f : mProgress.value + (operation.progress - mProgress.value) / 10;
                yield return null;
            }

            mProgress.value = 1;

            operation.allowSceneActivation = true;
            CurrentScene = targetScene;

            yield return new WaitForSeconds(1f);
            yield return new WaitUntil(() => {return CanTransition;});
            CanTransition = false;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetScene));

            action?.Invoke();

            yield return Fade(0);
            mProgress.value = 0;

            IsTransiting = false;
        }

        IEnumerator ReloadSceneCoroutine(string targetScene, Action action = null)
        {
            yield return Fade(1);
            yield return new WaitForSeconds(1f);

            IsTransiting = true;

            yield return SceneManager.UnloadSceneAsync(targetScene);

            var operation = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
            operation.allowSceneActivation = false;

            while (operation.progress <= 0.9f)
            {
                if (operation.progress == 0.9f && mProgress.value == 0.9f)
                {
                    break;
                }

                mProgress.value = (mProgress.value + (operation.progress - mProgress.value) / 10) > 0.85f ? 0.9f : mProgress.value + (operation.progress - mProgress.value) / 10;
                yield return null;
            }

            mProgress.value = 1;

            operation.allowSceneActivation = true;
            CurrentScene = targetScene;

            yield return new WaitForSeconds(1f);
            yield return new WaitUntil(() => {return CanTransition;});
            CanTransition = false;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetScene));

            action?.Invoke();

            yield return Fade(0);
            mProgress.value = 0;

            IsTransiting = false;
        }

        IEnumerator UnloadSceneCoroutine(string[] targetScene)
        {
            yield return Fade(1);
            yield return new WaitForSeconds(1f);

            IsTransiting = true;

            foreach (var scene in targetScene)
            {
                SceneManager.UnloadSceneAsync(scene);
            }
            
            CurrentScene = SceneManager.GetActiveScene().name;

            yield return new WaitForSeconds(1f);
            yield return Fade(0);
            mProgress.value = 0;

            IsTransiting = false;
        }

        IEnumerator SwitchSceneCoroutine(string targetScene, Action action = null)
        {
            yield return Fade(1);
            yield return new WaitForSeconds(1f);

            IsTransiting = true;

            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

            var operation = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
            operation.allowSceneActivation = false;

            while (operation.progress <= 0.9f)
            {
                if (operation.progress == 0.9f && mProgress.value == 0.9f)
                {
                    break;
                }

                mProgress.value = (mProgress.value + (operation.progress - mProgress.value) / 10) > 0.85f ? 0.9f : mProgress.value + (operation.progress - mProgress.value) / 10;
                yield return null;
            }

            mProgress.value = 1;

            operation.allowSceneActivation = true;
            CurrentScene = targetScene;

            yield return new WaitForSeconds(1f);
            yield return new WaitUntil(() => {return CanTransition;});
            CanTransition = false;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetScene));

            action?.Invoke();

            yield return Fade(0);
            mProgress.value = 0;

            IsTransiting = false;
        }

        IEnumerator Fade(float targetAlpha)
        {
            mCanvasGroup.blocksRaycasts = targetAlpha == 1;

            if (targetAlpha == 1)
            {
                while (!Mathf.Approximately(mCanvasGroup.alpha, targetAlpha))
                {
                    mCanvasGroup.alpha = Mathf.MoveTowards(mCanvasGroup.alpha, targetAlpha, 0.1f);
                    yield return new WaitForFixedUpdate();
                }

                mAnimator.enabled = true;
                mAnimator.speed = 1;
                mAnimator.Play("LoadingIn", 0, 0f);

                yield return new WaitUntil(() => {return mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.95f && mAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Appear");});
                mAnimator.speed = 0;
                yield return FadeText(mLabel, 1);
            }

            if (targetAlpha == 0)
            {
                yield return FadeText(mLabel, 0);

                mAnimator.enabled = true;
                mAnimator.speed = 1;
                mAnimator.Play("LoadingOut", 0, 0f);

                yield return new WaitUntil(() => {return mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.95f && mAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Disappear");});
                mAnimator.speed = 0;

                while (!Mathf.Approximately(mCanvasGroup.alpha, targetAlpha))
                {
                    mCanvasGroup.alpha = Mathf.MoveTowards(mCanvasGroup.alpha, targetAlpha, 0.1f);
                    yield return new WaitForFixedUpdate();
                }
            }

            mCanvasGroup.alpha = targetAlpha;
        }

        IEnumerator FadeText(TMP_Text text, float targetAlpha)
        {
            if (text.alpha == targetAlpha) yield break;

            while (!Mathf.Approximately(text.alpha, targetAlpha))
            {
                text.alpha = Mathf.MoveTowards(text.alpha, targetAlpha, 0.1f);
                yield return new WaitForFixedUpdate();
            }

            text.alpha = targetAlpha;
            yield break;
        }
    }

}