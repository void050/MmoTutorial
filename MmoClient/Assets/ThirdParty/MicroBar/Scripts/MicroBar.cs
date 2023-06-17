using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ****************************************************************************************************
// Controller for health bar. Handles animating health bar and healthbar appearance.
// Supports Image version and SpriteRenderer
// Requires visuals to be SpriteRenderers with DrawMode = Sliced
// or to be Image with ImageType = Filled
// Health bar uses width parameter to display HP amount in SpriteRenderers
// and fillAmount parameter to display HP amount in Images
// ****************************************************************************************************
namespace Microlight.MicroBar {
    enum HealthBarType { SpriteRenderer, Image }

    [ExecuteInEditMode]
    public class MicroBar : MonoBehaviour {
        //Type
        [SerializeField] HealthBarType _barType;

        //SpriteRenderer
        [SerializeField] SpriteRenderer _srBackground;
        [SerializeField] SpriteRenderer _srPrimaryBar;
        [SerializeField] SpriteRenderer _srGhostBar;

        //Image
        [SerializeField] Image _uiBackground;
        [SerializeField] Image _uiPrimaryBar;
        [SerializeField] Image _uiGhostBar;

        //Colors
        [SerializeField] Color _barPrimaryColor = new Color(1f, 1f, 1f);   // Color of main health bar
        [SerializeField] Color _barGhostHurtColor = new Color(1f, 0f, 0f);   // Color of ghost bar when hurt
        [SerializeField] Color _barGhostHealColor = new Color(1f, 1f, 1f);   // Color of ghost bar when healed
        [SerializeField] Color _barAdaptiveColor = new Color(1f, 0f, 0f);   // Color that health changes to as it gets lower
        [SerializeField] Color _barFlashingColor = new Color(1f, 0f, 0f);   // Color that health changes to as it flashes
        [SerializeField][Range(0f, 1f)] float _ghostBarAlpha = 0.6f;

        //General Settings
        [SerializeField] bool _adaptiveColor = true;   // Does health bar uses adaptive color based on current hp or single color for each bar
        [SerializeField] bool _flashingLowHealth = false;   // Will health bar flash in different color when low
        [SerializeField] bool _isAnimated = true;   // Is health bar animated
        [SerializeField] bool _useGhostBar = true;   // Is ghost bar used    
        [SerializeField] bool _dualGhostBars = false;   // Are ghost bars two separate bars for healing and damaging or single bar for both

        //Flashing Animation
        [SerializeField][Range(0f, 100f)] float _lowHealthThreshold = 25f;   // How low health needs to be from max HP (in %) to be considered low HP
        [SerializeField][Range(0f, 1f)] float _flashToTime = 0.1f;   // Time required to change color from default to low health color
        [SerializeField][Range(0f, 1f)] float _flashFromTime = 0.3f;   // Time requred to change color back to default from low health color

        //Bar Animation
        [SerializeField][Range(0.01f, 2f)] float _barFillDuration = 0.5f;   // Duration of bar moving
        [SerializeField][Range(0f, 2f)] float _damageFillDelay = 0f;   // How long will fill animation wait after shake animation
        [SerializeField][Range(0f, 2f)] float _healFillDelay = 0f;   // How long will fill animation wait before animating healing
        [SerializeField][Range(0f, 20f)] float _animationTriggerThreshold = 1f;   // % of HP that needs to change for animations to trigger  

        // Damage Shake
        [SerializeField] bool _damageShake = true;   // Will bar shake during damage
        [SerializeField] bool _adaptiveDamageShake = false;   // Will shake be adaptive based on change        
        [SerializeField][Range(0.01f, 5f)] float _damageShakeAmount = .2f;   // Bar shake amount, how much it moves around screen
        [SerializeField][Range(1, 200)] int _damageShakeIntensity = 40;   // Intensity of damage shake
        [SerializeField][Range(0.01f, 2f)] float _damageShakeDuration = 0.3f;   // How long damage shake will last
        [SerializeField] Vector2 _adaptiveShakeAmount = new Vector2(0f, 0.3f);   // Bar shake amount based on how much of health bar has been taken out
        [SerializeField] Vector2 _adaptiveShakeIntensity = new Vector2(10, 50);   // Bar shake intensity based on how much of health bar has been taken out
        [SerializeField][Range(0.1f, 1f)] float _adaptiveShakeThreshold = .5f;   // % of bar that needs to change to reach full adaptive value

        // Heal Scale
        [SerializeField] bool _healScale = true;   // Will bar scale during healing
        [SerializeField] bool _adaptiveHealScale = false;   // Will scale be adaptive based on change        
        [SerializeField][Range(1.01f, 2f)] float _healScaleAmount = 1.3f;   // How much health bar should increase during healing
        [SerializeField] Vector2 _adaptiveHealScaleAmount = new Vector2(1f, 1.3f);   // Heal scale amount based on how much of health bar has been healed
        [SerializeField][Range(0.1f, 1f)] float _adaptiveHealScaleThreshold = .5f;   // % of bar that needs to change to reach full adaptive value

        // Variables
        private static List<MicroBar> _animatedHealthBars = new List<MicroBar>();
        public static List<MicroBar> AnimatedHealthBars { get => _animatedHealthBars; private set => _animatedHealthBars = value; }
        public static event Action<MicroBar> OnBarFillEnd;
        public static event Action<MicroBar> OnScaleEnd;
        public static event Action<MicroBar> OnShakeEnd;
        public static event Action<MicroBar> OnFadeIn;
        public static event Action<MicroBar> OnFadeOut;
        public static event Action<MicroBar> OnAnimationEnd;
        bool _isInitalized = false;   // Safety system that gives out warning if health bar has not been initialized
        bool _isDamage = true;   // Is current animation damage or heal
        float _value = 1f;   // Current value of HP
        float _maxValue = 1f;   // Max value of HP
        float MaxValue {
            get => _maxValue;
            set {
                if(value < 1f) _maxValue = 1;
                else _maxValue = value;
            }
        }
        float _fillAmount = 1f;   // How much bar is filled (%)
        float _change = 0f;   // Difference between old and new bar position (change is in %)
        Color _backgroundBarColor;   // Stores color of background bar

        float _overrideShakeAmount = -1f;
        int _overrideShakeIntensity = -1;
        float _overrideShakeDuration = -1f;
        float _overrideHealScaleAmount = -1f;

        // Tweens
        Tween BarTween { get; set; }
        Sequence BarFlashingColorTween { get; set; }
        Tween ShakeTween { get; set; }
        Tween HealTween { get; set; }
        Tween BackgroundFadeTween { get; set; }
        Tween BarFadeTween { get; set; }
        Tween GhostBarFadeTween { get; set; }

        // Contains every method that is exposed 
        #region Public Methods
        /// <summary>
        /// Initializes health bar with max value
        /// </summary>
        /// <param name="maxValue">Max value</param>
        public void Initialize(float maxValue) {
            _isInitalized = true;
            MaxValue = maxValue;
            _value = MaxValue;
            UpdateHealthBar(maxValue, true);
        }

        /// <summary>
        /// Updates max value of health bar
        /// </summary>
        /// <param name="maxValue">Max health value</param>
        public void SetMaxHealth(float maxValue) {
            MaxValue = maxValue;
            UpdateHealthBar(maxValue, true);
        }

        /// <summary>
        /// Updates health bar.
        /// </summary>
        /// <param name="value">Value of HP</param>
        /// <param name="disableAnimate">If true, disables animation regardless of options and just updates bar</param>
        public void UpdateHealthBar(float value, bool disableAnimate = false) {
            if(!_isInitalized) Debug.LogWarning("Health bar has not been initalized.");   // Warn if bar has not been initialized        
            _value = Mathf.Clamp(value, 0f, MaxValue);   // Set values for health bar, without going over max or below min
            _change = _fillAmount;   // Store for further calculation of change
            _fillAmount = _value / MaxValue;   // At which % does bar needs to be with new value
            _change = Mathf.Abs(_change - _fillAmount);   // Calculate how much bar has moved

            // If bar is not animated
            if(!_isAnimated || disableAnimate || (_animationTriggerThreshold / 100) > _change) {
                SetBarValue();
                SetGhostBarValue();
            }
            else {   // Else animate it
                KillTweens();   // Stop any current animations

                // Is animation for damage or heal
                float currentValue = (_barType == HealthBarType.SpriteRenderer) ? _srPrimaryBar.size.x : _uiPrimaryBar.fillAmount;
                _isDamage = currentValue > _fillAmount;

                if(_useGhostBar) PrepareGhostBars();   // If using ghost bars, one of the bars needs to be set instantly

                if(_isDamage && _damageShake) DamageShake();   // Shake is played before fill animation if its damage
                else FillHealthBar();   // Else animate health bar 
            }
        }

        /// <summary>
        /// Fades health bar in or out
        /// </summary>
        /// <param name="fadeIn">True if fades in, False if fades out</param>
        /// <param name="duration">Over what time</param>
        public void FadeBar(bool fadeIn, float duration) {
            if(!_isInitalized) Debug.LogWarning("Health bar has not been initalized.");
            float backgroundFadeTo = fadeIn ? _backgroundBarColor.a : 0f;
            float barFadeTo = fadeIn ? 1f : 0f;
            float ghostFadeTo = fadeIn ? _ghostBarAlpha : 0f;

            if(_barType == HealthBarType.SpriteRenderer) {   // SpriteRenderer
                if(duration <= 0f) {   // If just want to set alpha
                    SetAlpha(_srBackground, backgroundFadeTo);
                    SetAlpha(_srPrimaryBar, barFadeTo);
                    if(_srGhostBar != null) SetAlpha(_srGhostBar, ghostFadeTo);
                }
                else {
                    AnimatedHealthBars.Add(this);   // Background bar
                    BackgroundFadeTween = _srBackground.DOFade(backgroundFadeTo, duration)
                        .OnComplete(CompleteAnimation);
                    AnimatedHealthBars.Add(this);   // Primary bar
                    BarFadeTween = _srPrimaryBar.DOFade(barFadeTo, duration)
                        .OnComplete(CompleteAnimation);
                    if(_useGhostBar) {   // Ghost bar
                        AnimatedHealthBars.Add(this);
                        GhostBarFadeTween = _srGhostBar.DOFade(ghostFadeTo, duration)
                            .OnComplete(CompleteAnimation);
                    }
                }
            }
            else {   // UI
                if(duration <= 0f) {   // If just want to set alpha
                    SetAlpha(_uiBackground, backgroundFadeTo);
                    SetAlpha(_uiPrimaryBar, barFadeTo);
                    if(_uiGhostBar != null) SetAlpha(_uiGhostBar, ghostFadeTo);
                }
                else {
                    AnimatedHealthBars.Add(this);   // Background bar
                    BackgroundFadeTween = _uiBackground.DOFade(backgroundFadeTo, duration)
                        .OnComplete(CompleteAnimation);
                    AnimatedHealthBars.Add(this);   // Primary bar
                    BarFadeTween = _uiPrimaryBar.DOFade(barFadeTo, duration)
                        .OnComplete(CompleteAnimation);
                    if(_useGhostBar) {   // Ghost bar
                        AnimatedHealthBars.Add(this);
                        GhostBarFadeTween = _uiGhostBar.DOFade(ghostFadeTo, duration)
                            .OnComplete(CompleteAnimation);
                    }
                }
            }

            void CompleteAnimation() {
                AnimatedHealthBars.Remove(this);
                if(fadeIn) {
                    OnFadeIn?.Invoke(this);
                    if(!AnimatedHealthBars.Contains(this)) OnAnimationEnd?.Invoke(this);
                }
                else {
                    OnFadeOut?.Invoke(this);
                    if(!AnimatedHealthBars.Contains(this)) OnAnimationEnd?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// If using damage shake, calling this function before UpdateHealthBar 
        /// allows for using override values of shake settings
        /// Can chain call like OverrideShake().UpdateHealthBar();
        /// Has no effect on adaptive damage shake
        /// </summary>
        /// <param name="adaptiveAmount">Pass value less than 0 to use default value.</param>
        /// <param name="adaptiveIntensity">Pass value less than 0 to use default value.</param>
        /// <param name="adaptiveDuration">Pass value less than 0 to use default value.</param>
        public MicroBar OverrideShake(float adaptiveAmount, int adaptiveIntensity, float adaptiveDuration) {
            _overrideShakeAmount = adaptiveAmount;
            _overrideShakeIntensity = adaptiveIntensity;
            _overrideShakeDuration = adaptiveDuration;
            return this;
        }

        /// <summary>
        /// If using heal scale, calling this function before UpdateHealthBar 
        /// allows for using override values of heal scale settings
        /// Can chain call like OverrideHealScale().UpdateHealthBar();
        /// Has no effect on adaptive heal scale
        /// </summary>
        /// <param name="adaptiveAmount">Pass value less than 0 to use default value.</param>
        public MicroBar OverrideHealScale(float adaptiveAmount) {
            _overrideHealScaleAmount = adaptiveAmount;
            return this;
        }
        #endregion

        #region Local
        // Clean up upon disabling or destroying object
        private void OnDisable() {
            KillTweens();
        }

        // At start sort some values
        private void Awake() {
            // Hide ghost bars if not using ghost bar
            if(!_useGhostBar) {
                if(_srGhostBar != null) _srGhostBar.gameObject.SetActive(false);
                if(_uiGhostBar != null) _uiGhostBar.gameObject.SetActive(false);
            }

            // Store background bar colors and check if all SpriteRenderers/Images are referenced
            if(_barType == HealthBarType.SpriteRenderer) {   // SpriteRenderer
                if(_srBackground != null) _backgroundBarColor = _srBackground.color;   // Store background bar color
                else Debug.LogWarning("SpriteRenderer for Background Bar missing!");
                if(_srPrimaryBar == null) Debug.LogWarning("SpriteRenderer for Bar missing!");
                if(_useGhostBar && _srGhostBar == null) Debug.LogWarning("SpriteRenderer for GhostBar missing!");
            }
            else {
                if(_uiBackground != null) _backgroundBarColor = _uiBackground.color;   // Store background bar color
                else Debug.LogWarning("Image for Background Bar missing!");
                if(_uiPrimaryBar == null) Debug.LogWarning("Image for Bar missing!");
                if(_useGhostBar && _uiGhostBar == null) Debug.LogWarning("Image for GhostBar missing!");
            }

            SetBarsColor();   // Sets appropriate color for each bar
        }

        // Sets primary bar based on values
        void SetBarValue() {
            if(_barType == HealthBarType.SpriteRenderer) _srPrimaryBar.size = new Vector2(_fillAmount, 1f);   // Sets bar value
            else _uiPrimaryBar.fillAmount = _fillAmount;
            SetBarsColor();
        }

        // Sets ghost bar based on values
        void SetGhostBarValue() {
            if(_barType == HealthBarType.SpriteRenderer) _srGhostBar.size = new Vector2(_fillAmount, 1f);   // Sets bar value
            else _uiGhostBar.fillAmount = _fillAmount;
            SetBarsColor();
        }

        // Updates bars colors based on settings
        void SetBarsColor() {
            if(_adaptiveColor) {   // If using adaptive colors
                if(_barType == HealthBarType.SpriteRenderer) _srPrimaryBar.color = Color.Lerp(_barAdaptiveColor, _barPrimaryColor, _srPrimaryBar.size.x);
                else _uiPrimaryBar.color = Color.Lerp(_barAdaptiveColor, _barPrimaryColor, _uiPrimaryBar.fillAmount);
            }
            else if(_flashingLowHealth) {
                bool lowHealth = (_value / MaxValue) * 100 <= _lowHealthThreshold;   // Is health considered low

                if(_barType == HealthBarType.SpriteRenderer) {
                    if(lowHealth && (!BarFlashingColorTween.IsActive())) {
                        BarFlashingColorTween = DOTween.Sequence();
                        BarFlashingColorTween.Append(_srPrimaryBar.DOColor(_barFlashingColor, _flashToTime));
                        BarFlashingColorTween.Append(_srPrimaryBar.DOColor(_barPrimaryColor, _flashFromTime));
                        BarFlashingColorTween.SetLoops(-1);
                        BarFlashingColorTween.Play();
                    }
                    else if(!lowHealth) {
                        if(BarFlashingColorTween.IsActive()) BarFlashingColorTween.Kill();
                        _srPrimaryBar.color = _barPrimaryColor;
                    }
                }
                else {
                    if(lowHealth && (!BarFlashingColorTween.IsActive())) {
                        BarFlashingColorTween = DOTween.Sequence();
                        BarFlashingColorTween.Append(_uiPrimaryBar.DOColor(_barFlashingColor, _flashToTime));
                        BarFlashingColorTween.Append(_uiPrimaryBar.DOColor(_barPrimaryColor, _flashFromTime));
                        BarFlashingColorTween.SetLoops(-1);
                        BarFlashingColorTween.Play();
                    }
                    else if(!lowHealth) {
                        if(BarFlashingColorTween.IsActive()) BarFlashingColorTween.Kill();
                        _uiPrimaryBar.color = _barPrimaryColor;
                    }
                }
            }
            else {
                // Primary bar
                if(_barType == HealthBarType.SpriteRenderer) _srPrimaryBar.color = _barPrimaryColor;
                else _uiPrimaryBar.color = _barPrimaryColor;
            }

            // Ghost bars
            if(_barType == HealthBarType.SpriteRenderer) {
                if(!_dualGhostBars) _srGhostBar.color = _barGhostHurtColor;
                else if(_isDamage) _srGhostBar.color = _barGhostHurtColor;
                else _srGhostBar.color = _barGhostHealColor;
                SetAlpha(_srGhostBar, _ghostBarAlpha);   // Update bar alpha
            }
            else {
                if(!_dualGhostBars) _uiGhostBar.color = _barGhostHurtColor;
                else if(_isDamage) _uiGhostBar.color = _barGhostHurtColor;
                else _uiGhostBar.color = _barGhostHealColor;
                SetAlpha(_uiGhostBar, _ghostBarAlpha);
            }
        }

        // If using ghost bars, sets ghost bars to prepare for animation
        void PrepareGhostBars() {
            // If its damage, primary bar is moved and ghost bar is animated else ghost bar is moved and primary bar is animated
            if(_isDamage) SetBarValue();   // Sets primary bar to current value
            else SetGhostBarValue();   // Sets ghost bar to current value
        }
        #endregion

        #region Animations
        // Animates health bar to desired value
        void FillHealthBar() {
            float delay = _isDamage ? _damageFillDelay : _healFillDelay;   // Decide delay

            if(_barType == HealthBarType.SpriteRenderer) {   // SpriteRenderer
                float currentValue;   // Used for tweening SpriteRenderer.size.x
                if(_isDamage && _useGhostBar) {   // If animating damage and ghost bar, then ghost bar is animated, and only then
                    AnimatedHealthBars.Add(this);
                    currentValue = _srGhostBar.size.x;
                    BarTween = DOTween.To(() => currentValue, x => currentValue = x, _fillAmount, _barFillDuration)
                        .SetDelay(delay)
                        .SetEase(Ease.OutQuart)
                        .OnUpdate(() => {
                            _srGhostBar.size = new Vector2(currentValue, 1f);
                            if(_adaptiveColor) SetBarsColor();   // Update color if using adaptive colors
                        })
                        .OnComplete(() => {
                            SetGhostBarValue();   // Finishes ghost bar position
                            AnimatedHealthBars.Remove(this);
                            OnBarFillEnd?.Invoke(this);
                            if(!AnimatedHealthBars.Contains(this)) OnAnimationEnd?.Invoke(this);
                        });
                }
                else {   // This can be damage or heal but can't be ghost bar
                    AnimatedHealthBars.Add(this);
                    if(_healScale && !_isDamage) HealScale();   // If need to play heal scale animation
                    currentValue = _srPrimaryBar.size.x;
                    BarTween = DOTween.To(() => currentValue, x => currentValue = x, _fillAmount, _barFillDuration)
                        .SetDelay(delay)
                        .SetEase(Ease.OutQuart)
                        .OnUpdate(() => {
                            _srPrimaryBar.size = new Vector2(currentValue, 1f);
                            if(_adaptiveColor) SetBarsColor();   // Update color if using adaptive colors
                        })
                        .OnComplete(() => {
                            SetBarValue();   // Finishes bar position
                            AnimatedHealthBars.Remove(this);
                            OnBarFillEnd?.Invoke(this);
                            if(!AnimatedHealthBars.Contains(this)) OnAnimationEnd?.Invoke(this);
                        });
                }
            }
            else {   // UI
                if(_isDamage && _useGhostBar) {   // If animating damage and ghost bar, then ghost bar is animated, and only then
                    AnimatedHealthBars.Add(this);
                    BarTween = _uiGhostBar.DOFillAmount(_fillAmount, _barFillDuration)
                        .SetDelay(delay)
                        .SetEase(Ease.OutQuart)
                        .OnUpdate(() => {
                            if(_adaptiveColor) SetBarsColor();   // Update color if using adaptive colors
                        })
                        .OnComplete(() => {
                            SetGhostBarValue();   // Finishes bar position
                            AnimatedHealthBars.Remove(this);
                            OnBarFillEnd?.Invoke(this);
                            if(!AnimatedHealthBars.Contains(this)) OnAnimationEnd?.Invoke(this);
                        });
                }
                else {   // This can be damage or heal but can't be ghost bar
                    AnimatedHealthBars.Add(this);
                    if(_healScale && !_isDamage) HealScale();   // If need to play heal scale animation
                    BarTween = _uiPrimaryBar.DOFillAmount(_fillAmount, _barFillDuration)
                        .SetDelay(delay)
                        .SetEase(Ease.OutQuart)
                        .OnUpdate(() => {
                            if(_adaptiveColor) SetBarsColor();   // Update color if using adaptive colors
                        })
                        .OnComplete(() => {
                            SetBarValue();   // Finishes bar position
                            AnimatedHealthBars.Remove(this);
                            OnBarFillEnd?.Invoke(this);
                            if(!AnimatedHealthBars.Contains(this)) OnAnimationEnd?.Invoke(this);
                        });
                }
            }
        }

        void DamageShake() {
            if(!_damageShake) return;   // Don't shake if option is disabled

            // Override settings
            float shakeAmount = _overrideShakeAmount < 0f ? _damageShakeAmount : _overrideShakeAmount;
            int shakeIntensity = _overrideShakeIntensity < 0f ? _damageShakeIntensity : _overrideShakeIntensity;
            float shakeDuration = _overrideShakeDuration < 0f ? _damageShakeDuration : _overrideShakeDuration;

            if(_adaptiveDamageShake) CalculateAdaptiveDamageShake();   // If using adaptive damage shake

            // Shake
            if(_barType == HealthBarType.SpriteRenderer) {
                Transform barTransform = _srPrimaryBar.GetComponent<Transform>();
                AnimatedHealthBars.Add(this);
                ShakeTween = barTransform.DOShakePosition(shakeDuration, new Vector3(1f, .5f, 0f) * shakeAmount, shakeIntensity)
                    .OnComplete(() => {
                        barTransform.localPosition = new Vector3(-0.5f, 0, 0);   // Reset position
                        AnimatedHealthBars.Remove(this);
                        FillHealthBar();   // Health bar needs to be animated after shake
                        OnShakeEnd?.Invoke(this);
                        if(!AnimatedHealthBars.Contains(this)) OnAnimationEnd?.Invoke(this);
                    });
            }
            else {
                RectTransform barRectTransform = _uiPrimaryBar.GetComponent<RectTransform>();
                AnimatedHealthBars.Add(this);
                ShakeTween = barRectTransform.DOShakeScale(shakeDuration, shakeAmount, shakeIntensity)
                    .OnComplete(() => {
                        barRectTransform.localScale = Vector3.one;   // Reset scale
                        AnimatedHealthBars.Remove(this);
                        FillHealthBar();   // Health bar needs to be animated after shake
                        OnShakeEnd?.Invoke(this);
                        if(!AnimatedHealthBars.Contains(this)) OnAnimationEnd?.Invoke(this);
                    });
            }

            // Reset adaptive shake after each call
            _overrideShakeAmount = -1f;
            _overrideShakeIntensity = -1;
            _overrideShakeDuration = -1f;

            #region Utility
            void CalculateAdaptiveDamageShake() {   // Calculation for adaptive shake
                shakeAmount = Mathf.Lerp(_adaptiveShakeAmount.x, _adaptiveShakeAmount.y, Mathf.Clamp(_change / _adaptiveShakeThreshold, 0f, 1f));
                shakeIntensity = (int)Mathf.Lerp(_adaptiveShakeIntensity.x, _adaptiveShakeIntensity.y, Mathf.Clamp(_change / _adaptiveShakeThreshold, 0f, 1f));
                shakeDuration = _damageShakeDuration;
            }
            #endregion
        }

        void HealScale() {
            if(!_healScale) return;   // Don't scale if option is disabled

            // Override settings
            float scaleAmount = _overrideHealScaleAmount < 0f ? _healScaleAmount : _overrideHealScaleAmount;

            if(_adaptiveHealScale) CalculateAdaptiveHealScale();   // If using adaptive heal scale

            // Scale
            if(_barType == HealthBarType.SpriteRenderer) {
                Transform barTransform = _srPrimaryBar.GetComponent<Transform>();
                barTransform.localScale = Vector3.one;   // Reset scale
                AnimatedHealthBars.Add(this);
                HealTween = barTransform.DOScaleY(scaleAmount, _barFillDuration)
                    .OnComplete(() => {
                        barTransform.localScale = Vector3.one;   // Reset scale
                        AnimatedHealthBars.Remove(this);
                        OnScaleEnd?.Invoke(this);
                        if(!AnimatedHealthBars.Contains(this)) OnAnimationEnd?.Invoke(this);
                    });
            }
            else {
                RectTransform barRectTransform = _uiPrimaryBar.GetComponent<RectTransform>();
                barRectTransform.localScale = Vector3.one;   // Reset scale
                AnimatedHealthBars.Add(this);
                HealTween = barRectTransform.DOScaleY(scaleAmount, _barFillDuration)
                    .OnComplete(() => {
                        barRectTransform.localScale = Vector3.one;   // Reset scale
                        AnimatedHealthBars.Remove(this);
                        OnScaleEnd?.Invoke(this);
                        if(!AnimatedHealthBars.Contains(this)) OnAnimationEnd?.Invoke(this);
                    });
            }

            // Reset adaptive heal scale after each call
            _overrideHealScaleAmount = -1f;

            #region Utility
            void CalculateAdaptiveHealScale() {   // Calculation for adaptive shake
                scaleAmount = Mathf.Lerp(_adaptiveHealScaleAmount.x, _adaptiveHealScaleAmount.y, Mathf.Clamp(_change / _adaptiveHealScaleThreshold, 0f, 1f));
            }
            #endregion
        }

        void KillTweens() {
            if(BarTween != null && BarTween.IsActive()) BarTween.Kill();   // Kills tween if already playing
            if(BarFlashingColorTween != null && BarFlashingColorTween.IsActive()) BarFlashingColorTween.Kill();
            if(ShakeTween != null && ShakeTween.IsActive()) ShakeTween.Kill();
            if(HealTween != null && HealTween.IsActive()) HealTween.Kill();
            if(BackgroundFadeTween != null && BackgroundFadeTween.IsActive()) BackgroundFadeTween.Kill();
            if(BarFadeTween != null && BarFadeTween.IsActive()) BarFadeTween.Kill();
            if(GhostBarFadeTween != null && GhostBarFadeTween.IsActive()) GhostBarFadeTween.Kill();
            ResetScaleAndPosition();
        }

        void ResetScaleAndPosition() {
            if(_srPrimaryBar != null) _srPrimaryBar.GetComponent<Transform>().localPosition = new Vector3(-0.5f, 0f, 0f);
            if(_srPrimaryBar != null) _srPrimaryBar.GetComponent<Transform>().localScale = Vector3.one;
            if(_uiPrimaryBar != null) _uiPrimaryBar.GetComponent<RectTransform>().localScale = Vector3.one;
        }
        #endregion

        #region Utility
        // Sets alpha value of color
        void SetAlpha(Image image, float value) {
            Color color = image.color;
            color.a = value;
            image.color = color;
        }
        void SetAlpha(SpriteRenderer renderer, float value) {
            Color color = renderer.color;
            color.a = value;
            renderer.color = color;
        }
        #endregion

#if UNITY_EDITOR
        [MenuItem("GameObject/Microlight/SpriteRenderer Health Bar")]
        private static void AddSpriteRendererHealthBar() {
            // Get prefab
            GameObject go = MicroBarAssetUtilities.GetPrefab("SpriteRendererMicroBar");
            if(go == null) return;

            go = Instantiate(go);   // Instantiate
            go.name = "HealthBar";   // Change name
            if(Selection.activeGameObject != null) {   // Make child if some object is selected
                go.transform.SetParent(Selection.activeGameObject.transform);
            }
        }

        [MenuItem("GameObject/Microlight/UI Image Health Bar")]
        private static void AddImageHealthBar() {
            // Get prefab
            GameObject go = MicroBarAssetUtilities.GetPrefab("UIImageMicroBar");
            if(go == null) return;

            go = Instantiate(go);   // Instantiate
            go.name = "HealthBar";   // Change name
            if(Selection.activeGameObject != null) {   // Make child if some object is selected
                go.transform.SetParent(Selection.activeGameObject.transform, false);
            }
        }

        [MenuItem("GameObject/Microlight/UI Image Health Bar", true)]
        private static bool AddImageHealthBar_Validate() {
            return Selection.activeGameObject && Selection.activeGameObject.GetComponentInParent<Canvas>();
        }
#endif
    }

    #region Custom Editor
#if UNITY_EDITOR
    // ****************************************************************************************************
    // Custom editor for MicroBar. Used only for editor
    // ****************************************************************************************************
    [CustomEditor(typeof(MicroBar))]
    public class MicroBar_Editor : Editor {
        /*
        float _startHealthAmount = 100f;   // Start health during health bar testing
        float _healthChangeAmount = -10f;   // How much will health change on test
        float HP = 100f;
        */

        public override void OnInspectorGUI() {
            serializedObject.Update();

            // Store serialized properties
            SerializedProperty barType = serializedObject.FindProperty("_barType");
            SerializedProperty adaptiveColor = serializedObject.FindProperty("_adaptiveColor");
            SerializedProperty flashingLowHealth = serializedObject.FindProperty("_flashingLowHealth");
            SerializedProperty isAnimated = serializedObject.FindProperty("_isAnimated");
            SerializedProperty ghostBar = serializedObject.FindProperty("_useGhostBar");
            SerializedProperty dualGhostBars = serializedObject.FindProperty("_dualGhostBars");
            SerializedProperty damageShake = serializedObject.FindProperty("_damageShake");
            SerializedProperty healScale = serializedObject.FindProperty("_healScale");
            SerializedProperty adaptiveDamageShake = serializedObject.FindProperty("_adaptiveDamageShake");
            SerializedProperty adaptiveHealScale = serializedObject.FindProperty("_adaptiveHealScale");

            EditorGUILayout.LabelField("Type", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(barType);   // Select type of health bar (SpriteRenderer or Image)

            // References to SpriteRenderers and UI Images
            EditorGUILayout.Space();
            switch((HealthBarType)barType.enumValueIndex) {
                case HealthBarType.SpriteRenderer:
                    EditorGUILayout.LabelField("Sprite Renderer", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_srBackground"), new GUIContent("SpriteRenderer Background"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_srPrimaryBar"), new GUIContent("SpriteRenderer Bar"));
                    if(ghostBar.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_srGhostBar"), new GUIContent("SpriteRenderer GhostBar"));
                    break;
                case HealthBarType.Image:
                    EditorGUILayout.LabelField("Image", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_uiBackground"), new GUIContent("Image Background"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_uiPrimaryBar"), new GUIContent("Image Bar"));
                    if(ghostBar.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_uiGhostBar"), new GUIContent("Image GhostBar"));
                    break;
                default: break;
            }

            // Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
            if(!flashingLowHealth.boolValue) EditorGUILayout.PropertyField(adaptiveColor, new GUIContent("Adaptive Color",
                "With adaptive color lerps between full and empty color based on fill amount. Without adaptive color every bar has same color whole time. (Disables Flashing Low Health)"));
            else {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(adaptiveColor, new GUIContent("Adaptive Color",
                    "With adaptive color lerps between full and empty color based on fill amount. Without adaptive color every bar has same color whole time. (Disables Flashing Low Health)"));
                GUI.enabled = true;
            }
            if(!adaptiveColor.boolValue) EditorGUILayout.PropertyField(flashingLowHealth, new GUIContent("Flashing Low Health",
                "Flashing low health alternates between main and secondary color when low is below certain threshold. (Disables Adaptive Color)"));
            else {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(flashingLowHealth, new GUIContent("Flashing Low Health",
                "Flashing low health alternates between main and secondary color when low is below certain threshold. (Disables Adaptive Color)"));
                GUI.enabled = true;
            }
            EditorGUILayout.PropertyField(isAnimated, new GUIContent("Animated", "Will health bar be instantly updated or animated."));
            if(isAnimated.boolValue) EditorGUILayout.PropertyField(ghostBar, new GUIContent("Use Ghost Bar",
                "Will second health bar be displayed during animation? (showing amount healed and amount hurt)."));
            if(isAnimated.boolValue && ghostBar.boolValue && !adaptiveColor.boolValue) EditorGUILayout.PropertyField(dualGhostBars, new GUIContent("Dual Ghost Bars",
                "Allows for heal bar to be different color than hurt bar."));

            // Disable settings that can't be
            if(!isAnimated.boolValue) ghostBar.boolValue = false;   // Disable ghost bar if bar is not animated
            if(!isAnimated.boolValue) flashingLowHealth.boolValue = false;   // Disable flashing health bar if bar is not animated
            if(!ghostBar.boolValue) dualGhostBars.boolValue = false;   // Disable dual ghost bars if ghost bar is disabled
            if(adaptiveColor.boolValue) dualGhostBars.boolValue = false;   // Disable dual ghost bars if using adaptive colors
            if(adaptiveColor.boolValue) flashingLowHealth.boolValue = false;   // Disable flashing health bar if adaptive color is enabled
            if(flashingLowHealth.boolValue) adaptiveColor.boolValue = false;   // Disable adaptive color if flashing health bar is enabled
            if(!isAnimated.boolValue) damageShake.boolValue = false;   // Disable damage shake if not animated
            if(!isAnimated.boolValue) healScale.boolValue = false;   // Disable heal scale if not animated

            if(!damageShake.boolValue) adaptiveDamageShake.boolValue = false;
            if(!healScale.boolValue) adaptiveHealScale.boolValue = false;

            // Colors
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
            // Primary
            if(adaptiveColor.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_barPrimaryColor"), new GUIContent("Full Health Color", "Color of the bar when health is full."));
            else if(flashingLowHealth.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_barPrimaryColor"), new GUIContent("Health Bar Color", "Color of the health bar."));
            else if(ghostBar.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_barPrimaryColor"), new GUIContent("Health Bar Color", "Color of the main health bar."));
            else EditorGUILayout.PropertyField(serializedObject.FindProperty("_barPrimaryColor"), new GUIContent("Bar Color", "Color of the health bar."));
            // Secondary
            if(adaptiveColor.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_barAdaptiveColor"), new GUIContent("Low Health Color", "Color of the bar when health is low."));
            else if(flashingLowHealth.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_barFlashingColor"), new GUIContent("Flash Health Color", "Color of the bar when bar flashes."));
            if(ghostBar.boolValue && !dualGhostBars.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_barGhostHurtColor"), new GUIContent("Ghost Bar Color", "Color of the ghost bar."));
            else if(ghostBar.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_barGhostHurtColor"), new GUIContent("Ghost Hurt Color", "Color of the ghost bar when hurt."));
            if(dualGhostBars.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_barGhostHealColor"), new GUIContent("Ghost Heal Color", "Color of the ghost bar when healed."));
            // Transparency
            if(isAnimated.boolValue && ghostBar.boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_ghostBarAlpha"), new GUIContent("Ghost Bar Alpha", "Transparency ghost bar is."));

            // Bar animation
            if(isAnimated.boolValue) EditorGUILayout.Space();
            if(isAnimated.boolValue) EditorGUILayout.LabelField("Bar Animation", EditorStyles.boldLabel);
            if(isAnimated.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_barFillDuration"), new GUIContent("Fill Duration",
                "Duration for bar to reach set value."));
            if(isAnimated.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_damageFillDelay"), new GUIContent("Damage Fill Delay",
                "Delay before fill animation starts."));
            if(isAnimated.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_healFillDelay"), new GUIContent("Heal Fill Delay",
                "Delay before fill animation starts."));
            if(isAnimated.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_animationTriggerThreshold"), new GUIContent("Animation Trigger Threshold",
                "% HP needs to change for animation to trigger."));

            // Flashing bar
            if(flashingLowHealth.boolValue) EditorGUILayout.Space();
            if(flashingLowHealth.boolValue) EditorGUILayout.LabelField("Flashing Low Health", EditorStyles.boldLabel);
            if(flashingLowHealth.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_lowHealthThreshold"), new GUIContent("Low Health Threshold",
                "At which point is health considered as low health (%)."));
            if(flashingLowHealth.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_flashToTime"), new GUIContent("Flash To Time",
                "Time to change from default color to flashing color."));
            if(flashingLowHealth.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_flashFromTime"), new GUIContent("Flash From Time",
                "Time to change from flashing color to default color."));

            // Damage Shake
            if(isAnimated.boolValue) EditorGUILayout.Space();
            if(isAnimated.boolValue) EditorGUILayout.LabelField("Damage Shake", EditorStyles.boldLabel);
            if(isAnimated.boolValue) EditorGUILayout.PropertyField(damageShake, new GUIContent("Shake",
                "Will health bar shake during damage."));
            if(damageShake.boolValue) EditorGUILayout.PropertyField(adaptiveDamageShake, new GUIContent("Adaptive Damage Shake",
                "Allows to change shake settings based on % of health bar changed."));
            if(damageShake.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_damageShakeDuration"), new GUIContent("Damage Shake Duration",
                "How long will damage shake last (in seconds)."));
            // Non-adaptive
            if(damageShake.boolValue && !adaptiveDamageShake.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_damageShakeAmount"), new GUIContent("Damage Shake Amount",
                "How much will bar move around during damage shake."));
            if(damageShake.boolValue && !adaptiveDamageShake.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_damageShakeIntensity"), new GUIContent("Damage Shake Intensity",
                "Vibration of damage shake."));
            // Adaptive
            if(adaptiveDamageShake.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_adaptiveShakeAmount"), new GUIContent("Adaptive Damage Shake Amount",
                "Lowest shake amount when bar changes 0% and largest shake amount when bar changes threshold % amount."));
            if(adaptiveDamageShake.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_adaptiveShakeIntensity"), new GUIContent("Adaptive Damage Shake Intensity",
                "Lowest shake intensity when bar changes 0% and largest shake intensity when bar changes threshold % amount."));
            if(adaptiveDamageShake.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_adaptiveShakeThreshold"), new GUIContent("Adaptive Damage Shake Threshold",
                "At how much bar % change will adaptive shake reach full."));

            // Heal Scale
            if(isAnimated.boolValue) EditorGUILayout.Space();
            if(isAnimated.boolValue) EditorGUILayout.LabelField("Heal Scale", EditorStyles.boldLabel);
            if(isAnimated.boolValue) EditorGUILayout.PropertyField(healScale, new GUIContent("Heal Scale",
                "Will health bar scale up during healing."));
            if(healScale.boolValue) EditorGUILayout.PropertyField(adaptiveHealScale, new GUIContent("Adaptive Heal Scale",
                "Allows to change shake settings based on % of health bar changed."));
            // Non-adaptive
            if(healScale.boolValue && !adaptiveHealScale.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_healScaleAmount"), new GUIContent("Heal Scale Intensity",
                "How much health bar will scale up."));
            // Adaptive
            if(adaptiveHealScale.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_adaptiveHealScaleAmount"), new GUIContent("Adaptive Heal Scale Amount",
                "Lowest scale amount when bar changes 0% and largest scale amount when bar changes threshold % amount."));
            if(adaptiveHealScale.boolValue) EditorGUILayout.PropertyField(serializedObject.FindProperty("_adaptiveHealScaleThreshold"), new GUIContent("Adaptive Heal Scale Threshold",
                "At how much bar % change will adaptive heal scale reach full."));

            // HealthBar Test
            /*
            HealthBar bar = (HealthBar)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Test", EditorStyles.boldLabel);

            // Set health            
            EditorGUIUtility.labelWidth = 10f;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Setup");
            _startHealthAmount = EditorGUILayout.Slider(_startHealthAmount, 1f, 100f);
            if (GUILayout.Button("Set Health")) {
                HP = _startHealthAmount;
                bar.Initialize(100f);
                bar.UpdateHealthBar(_startHealthAmount, true);
            }
            EditorGUILayout.EndHorizontal();

            // Test
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();        
            EditorGUILayout.LabelField("Change HP");
            _healthChangeAmount = EditorGUILayout.Slider(_healthChangeAmount, -100f, 100f);
            if (GUILayout.Button("Test")) {
                HP += _healthChangeAmount;
                bar.UpdateHealthBar(HP);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0f;
            */

            serializedObject.ApplyModifiedProperties();   // Apply changes
        }
    }
#endif
    #endregion
}