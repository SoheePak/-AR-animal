using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Top level UI script that manages the UI
/// </summary>

public class TheUI : MonoBehaviour
{
    public UIPanel pause_panel;
    public UIPanel game_over_panel;
    public UIPanel damage_fx;

    public AudioClip ui_sound;

    public Image speaker_btn;
    public Sprite speaker_on;
    public Sprite speaker_off;

    private Canvas canvas;
    private RectTransform rect;

    private static TheUI _instance;

    void Awake()
    {
        _instance = this;
        canvas = GetComponent<Canvas>();
        rect = GetComponent<RectTransform>();
        
    }

    private void Start()
    {
        canvas.worldCamera = TheCamera.GetCamera();

        PlayerCharacter.Get().onDamaged += DoDamageFX;
    }

    void Update()
    {
        pause_panel.SetVisible(TheGame.Get().IsPaused());
        speaker_btn.sprite = PlayerData.Get().master_volume > 0.1f ? speaker_on : speaker_off;
    }

    public void DoDamageFX()
    {
        StartCoroutine(DamageFXRun());
    }

    private IEnumerator DamageFXRun()
    {
        damage_fx.Show();
        yield return new WaitForSeconds(1f);
        damage_fx.Hide();
    }

    public void CancelSelection()
    {
        EquipBar.Get().CancelSelection();
        CraftBar.Get().CancelSelection();
        InventoryBar.Get().CancelSelection();
        PlayerCharacter.Get().CancelConstruction();
    }

    public void ShowGameOver()
    {
        game_over_panel.Show();
    }

    public void OnClickPause()
    {
        if (TheGame.Get().IsPaused())
            TheGame.Get().Unpause();
        else
            TheGame.Get().Pause();

        TheAudio.Get().PlaySFX("UI", ui_sound);
    }

    public void OnClickSave()
    {
        TheGame.Get().Save(); 
    }

    public void OnClickLoad()
    {
        StartCoroutine(LoadRoutine());
    }

    public void OnClickNew()
    {
        StartCoroutine(NewRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        BlackPanel.Get().Show();

        yield return new WaitForSeconds(1f);

        PlayerData.Unload(); //Make sure to unload first, or it won't load if already loaded
        PlayerData.LoadLast();
        SceneNav.GoTo(PlayerData.Get().current_scene);
    }

    private IEnumerator NewRoutine()
    {
        BlackPanel.Get().Show();

        yield return new WaitForSeconds(1f);

        PlayerData.NewGame();
        SceneNav.GoTo(PlayerData.Get().current_scene);
    }

    public void OnClickCraft()
    {
        CancelSelection();
        CraftBar.Get().ToggleBar();
    }

    public void OnClickMusicToggle()
    {
        PlayerData.Get().master_volume = PlayerData.Get().master_volume > 0.1f ? 0f : 1f;
        TheAudio.Get().RefreshVolume();
    }

    //Convert a screen position (like mouse) to a anchored position in the canvas
    public Vector2 ScreenPointToCanvasPos(Vector2 pos)
    {
        Vector2 localpoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, pos, canvas.worldCamera, out localpoint);
        return localpoint;
    }

    public static TheUI Get() {
        return _instance;
    }
}
