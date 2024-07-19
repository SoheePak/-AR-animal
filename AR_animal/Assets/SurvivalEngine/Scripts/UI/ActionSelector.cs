using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ActionSelector is the panel that popup and allow you to pick an action to do when you click on a selectable
/// </summary>

public class ActionSelector : MonoBehaviour
{
    public ActionSelectorButton[] buttons;

    private Animator animator;
    private bool visible = false;

    private PlayerCharacter character;
    private Selectable select;
    private ItemSlot slot;

    private static ActionSelector _instance;

    void Awake()
    {
        _instance = this;
        animator = GetComponent<Animator>();
        gameObject.SetActive(false);

        foreach (ActionSelectorButton button in buttons)
        {
            button.onClick += OnClickAction;
        }
    }

    private void Start()
    {
        //PlayerControlsMouse.Get().onClick += OnMouseClick;
        PlayerControlsMouse.Get().onRightClick += OnMouseClick;

    }

    void Update()
    {
        if (visible && character != null && select != null)
        {
            float dist = (transform.position - character.transform.position).magnitude;
            if (dist > select.use_range)
            {
                Hide();
            }
        }

        if (visible)
        {
            Vector3 dir = TheCamera.Get().GetFacingFront();
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        if (visible && select == null)
            Hide();
    }

    private void RefreshPanel()
    {
        foreach (ActionSelectorButton button in buttons)
            button.Hide();

        if (select != null)
        {
            int index = 0;
            foreach (SAction action in select.actions)
            {
                if (index < buttons.Length && action.CanDoAction(character, select))
                {
                    ActionSelectorButton button = buttons[index];
                    button.SetButton(action);
                    index++;
                }
            }
        }

        if (slot != null)
        {
            int index = 0;
            foreach (SAction action in slot.GetItem().actions)
            {
                if (index < buttons.Length && action.CanDoAction(character, slot))
                {
                    ActionSelectorButton button = buttons[index];
                    button.SetButton(action);
                    index++;
                }
            }
        }
    }

    public void Show(PlayerCharacter character, Selectable select)
    {
        if (select != null && character != null)
        {
            if (!visible || this.select != select || this.character != character)
            {
                this.select = select;
                this.character = character;
                this.slot = null;
                visible = true;
                RefreshPanel();
                animator.Rebind();
                //animator.SetTrigger("Show");
                transform.position = select.transform.position;
                gameObject.SetActive(true);
            }
        }
    }

    public void Show(PlayerCharacter character, ItemSlot slot)
    {
        if (slot != null && character != null)
        {
            if (!visible || this.slot != slot || this.character != character)
            {
                this.slot = slot;
                this.character = character;
                this.select = null;
                visible = true;
                RefreshPanel();
                animator.Rebind();
                //animator.SetTrigger("Show");
                transform.position = slot.transform.position;
                gameObject.SetActive(true);
            }
        }
    }

    public void Hide()
    {
        if (visible)
        {
            select = null;
            character = null;
            visible = false;
            animator.SetTrigger("Hide");
            Invoke("AfterHide", 1f);
        }
    }

    private void AfterHide()
    {
        if(!visible)
            gameObject.SetActive(false);
    }

    public void OnClickAction(SAction action)
    {
        if (visible)
        {
            if (action != null && select != null && character != null)
            {
                character.FaceTorward(select.transform.position);
                if (action.CanDoAction(character, select))
                    action.DoAction(character, select);
                Hide();
            }

            if (action != null && slot != null && character != null)
            {
                if (action.CanDoAction(character, slot))
                    action.DoAction(character, slot);
                Hide();
            }
        }
    }

    private void OnMouseClick(Vector3 pos)
    {
        Hide();
    }

    public Selectable GetSelectable()
    {
        return select;
    }

    public bool IsVisible()
    {
        return visible;
    }

    public static ActionSelector Get()
    {
        return _instance;
    }
}
