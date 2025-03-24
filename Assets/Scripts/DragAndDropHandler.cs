using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragAndDropHandler : MonoBehaviour {

    [SerializeField] private UIItemSlot cursorSlot = null;
    private ItemSlot cursorItemSlot;

    [SerializeField] private GraphicRaycaster m_Raycaster = null;
    private PointerEventData m_PointerEventData;
    [SerializeField] private EventSystem m_EventSystem = null;

    World world;

    private void Start() {
        world = GameObject.Find("World").GetComponent<World>();

        cursorItemSlot = new ItemSlot(cursorSlot);
    }

    private void Update() {
        if (!world.inUI) {
            return;
        }

        cursorSlot.transform.position = Input.mousePosition;

        if (Input.GetMouseButtonDown(0)) {
            HandleSlotClick(CheckForSlot());
        }
    }

    private void HandleSlotClick(UIItemSlot clickedSlot) {
        if (clickedSlot == null)
            return;

        if (!cursorSlot.HasItem && !clickedSlot.HasItem)
            return;

        if (clickedSlot.itemSlot.isCreative) {
            cursorItemSlot.EmptySlot();
            cursorItemSlot.InsertStack(clickedSlot.itemSlot.stack);
        }

        // Agafa el stack i el pot moure
        if (!cursorSlot.HasItem && clickedSlot.HasItem) {
            cursorItemSlot.InsertStack(clickedSlot.itemSlot.TakeAll());
            return;
        }

        // Deixa el stack i deixa de moure's
        if (cursorSlot.HasItem && !clickedSlot.HasItem) {
            clickedSlot.itemSlot.InsertStack(cursorItemSlot.TakeAll());
            return;
        }

        // Canvia el stack seleccionat per el clicat
        if (cursorSlot.HasItem && clickedSlot.HasItem) {
            if (cursorSlot.itemSlot.stack.id != clickedSlot.itemSlot.stack.id) {
                ItemStack oldCursorSlot = cursorSlot.itemSlot.TakeAll();
                ItemStack oldSlot = clickedSlot.itemSlot.TakeAll();

                clickedSlot.itemSlot.InsertStack(oldCursorSlot);
                cursorSlot.itemSlot.InsertStack(oldSlot);
            } else {
                byte clickedSlotItemID = clickedSlot.itemSlot.stack.id;
                int maxStackSize = world.blockTypes[clickedSlotItemID].maxStackSize;
                int clickedSlotAmount = clickedSlot.itemSlot.stack.amount;
                int cursorSlotAmount = cursorSlot.itemSlot.stack.amount;
                int totalAmount = clickedSlotAmount + cursorSlotAmount;

                if (clickedSlotAmount >= maxStackSize) {
                    // Si el slot clicado ya está lleno, intercambiamos los stacks
                    ItemStack oldCursorSlot = cursorSlot.itemSlot.TakeAll();
                    ItemStack oldSlot = clickedSlot.itemSlot.TakeAll();

                    clickedSlot.itemSlot.InsertStack(oldCursorSlot);
                    cursorSlot.itemSlot.InsertStack(oldSlot);
                    return;
                }

                if (totalAmount <= maxStackSize) {
                    // Si la suma cabe en el slot clicado
                    clickedSlot.itemSlot.stack.amount = totalAmount;
                    cursorItemSlot.EmptySlot();

                    // Actualizar la UI
                    clickedSlot.UpdateSlot();
                    cursorSlot.UpdateSlot();
                } else {
                    // Si excede el máximo, llenamos el slot clicado y dejamos el resto en el cursor
                    clickedSlot.itemSlot.stack.amount = maxStackSize;
                    cursorSlot.itemSlot.stack.amount = totalAmount - maxStackSize;

                    // Actualizar la UI
                    clickedSlot.UpdateSlot();
                    cursorSlot.UpdateSlot();
                }
            }
            // else { // TEST DE ELSE - junta els stacks fins arribar a 64
            //    byte clickedSlotItemID = clickedSlot.itemSlot.stack.id;
            //    int maxStackSize = world.blockTypes[clickedSlotItemID].maxStackSize;

            //    ItemStack oldCursorSlot = cursorSlot.itemSlot.TakeAll();
            //    ItemStack oldSlot = clickedSlot.itemSlot.TakeAll();
            //    int combinedStacksAmount = oldSlot.amount + oldCursorSlot.amount;

            //    if (combinedStacksAmount > maxStackSize) {
            //        ItemStack combinedStackCursorSlot = new ItemStack(clickedSlotItemID, maxStackSize);
            //        ItemStack combinedStackLeftover = new ItemStack(clickedSlotItemID, combinedStacksAmount - maxStackSize); // El que sobra de la suma dels dos

            //        clickedSlot.itemSlot.InsertStack(combinedStackCursorSlot);
            //        cursorSlot.itemSlot.InsertStack(combinedStackLeftover);
            //        return;
            //    } else {
            //        ItemStack combinedClickedSlotStack = new ItemStack(clickedSlotItemID, combinedStacksAmount);
            //        clickedSlot.itemSlot.InsertStack(combinedClickedSlotStack);
            //        return;
            //    }
            //}
        }
    }

    private UIItemSlot CheckForSlot() {
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointerEventData, results);

        foreach (RaycastResult result in results) {
            if (result.gameObject.tag == "UIItemSlot")
                return result.gameObject.GetComponent<UIItemSlot>();
        }

        return null;
    }

}
