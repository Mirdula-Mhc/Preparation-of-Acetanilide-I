using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// One Screen Space draggable UI -> one World Space UI drop target.
/// Matching is done using matchID.
/// </summary>
[Serializable]
public class DragDropItem
{
    [Header("MATCHING")]
    [Tooltip("Unique ID used to match the draggable with its correct drop target.")]
    public string matchID;

    [Header("SCREEN SPACE - DRAGGABLE")]
    [Tooltip("The UI Image that will be dragged from the Screen Space Canvas.")]
    public RectTransform draggableImage;

    [Header("WORLD SPACE - DROP TARGET")]
    [Tooltip("World Space UI RectTransform. No Collider is required.")]
    public RectTransform worldDropTarget;

    [Header("WORLD SPACE - RESULT")]
    [Tooltip("World Space UI/Text to enable after this item is correctly dropped.")]
    public GameObject worldText;

   


    // Runtime data
    [HideInInspector] public Vector3 originalPosition;
    [HideInInspector] public Quaternion originalRotation;
    [HideInInspector] public Vector3 originalScale;
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public int originalSiblingIndex;
    [HideInInspector] public bool isCompleted;
}


/// <summary>
/// Screen Space Canvas UI -> World Space Canvas UI Drag & Drop Manager.
///
/// No Collider is required on the World Space drop target.
///
/// Add this script to one GameObject and configure everything
/// from the Inspector.
/// </summary>
public class DragDropManager : MonoBehaviour
{
    // =========================================================
    // CANVAS REFERENCES
    // =========================================================

    [Header("CANVAS REFERENCES")]

    [Tooltip("Screen Space Canvas containing the draggable UI images.")]
    [SerializeField]
    private Canvas screenSpaceCanvas;

    [Tooltip(
        "World Space Canvas containing the drop targets. " +
        "If empty, the script will try to find it automatically."
    )]
    [SerializeField]
    private Canvas worldSpaceCanvas;


    // =========================================================
    // DRAG ITEMS
    // =========================================================

    [Header("DRAG & DROP ITEMS")]

    [Tooltip(
        "Add one element for each draggable image and its matching World Space target."
    )]
    [SerializeField]
    private List<DragDropItem> dragDropItems =
        new List<DragDropItem>();


    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("DRAG SETTINGS")]

    [Tooltip(
        "If enabled, the image keeps the same pointer offset while dragging."
    )]
    [SerializeField]
    private bool keepPointerOffset = true;


    [Tooltip(
        "If enabled, dragged UI moves to the front of its Canvas."
    )]
    [SerializeField]
    private bool bringToFront = true;


    [Tooltip(
        "Enable this to see useful drag/drop messages in Console."
    )]
    [SerializeField]
    private bool debugLogs = false;

    [Header("EVENTS")]
    [SerializeField] private UnityEvent onAllDragComplet;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        // Automatically find World Space Canvas if not assigned.
        if (worldSpaceCanvas == null)
        {
            Canvas[] canvases =
                FindObjectsByType<Canvas>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (Canvas canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.WorldSpace)
                {
                    worldSpaceCanvas = canvas;
                    break;
                }
            }
        }

        InitializeWorldTexts();
        InitializeDraggables();
    }


    // =========================================================
    // INITIALIZE WORLD TEXTS
    // =========================================================

    private void InitializeWorldTexts()
    {
        foreach (DragDropItem item in dragDropItems)
        {
            if (item == null)
                continue;

            if (item.worldText != null)
            {
                item.worldText.SetActive(false);
            }
        }
    }


    // =========================================================
    // INITIALIZE DRAGGABLES
    // =========================================================

    private void InitializeDraggables()
    {
        foreach (DragDropItem item in dragDropItems)
        {
            if (item == null)
                continue;


            // -------------------------------------------------
            // Check draggable
            // -------------------------------------------------

            if (item.draggableImage == null)
            {
                Debug.LogWarning(
                    "[DragDropManager] Missing Draggable Image for: " +
                    item.matchID
                );

                continue;
            }


            // -------------------------------------------------
            // Store original transform
            // -------------------------------------------------

            item.originalPosition =
                item.draggableImage.position;

            item.originalRotation =
                item.draggableImage.rotation;

            item.originalScale =
                item.draggableImage.localScale;

            item.originalParent =
                item.draggableImage.parent;

            item.originalSiblingIndex =
                item.draggableImage.GetSiblingIndex();

            item.isCompleted = false;


            // -------------------------------------------------
            // Canvas Group
            // -------------------------------------------------

            CanvasGroup canvasGroup =
                item.draggableImage.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup =
                    item.draggableImage.gameObject
                    .AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = true;


            // -------------------------------------------------
            // Drag Handler
            // -------------------------------------------------

            DragHandler handler =
                item.draggableImage.GetComponent<DragHandler>();

            if (handler == null)
            {
                handler =
                    item.draggableImage.gameObject
                    .AddComponent<DragHandler>();
            }


            handler.Initialize(
                this,
                item,
                screenSpaceCanvas,
                canvasGroup,
                keepPointerOffset,
                bringToFront
            );


            // -------------------------------------------------
            // Validate Drop Target
            // -------------------------------------------------

            if (item.worldDropTarget == null)
            {
                Debug.LogWarning(
                    "[DragDropManager] Missing World Drop Target for: " +
                    item.matchID
                );
            }
        }
    }


    // =========================================================
    // HANDLE DROP
    // =========================================================

    public void HandleDrop(
        DragDropItem item,
        PointerEventData eventData
    )
    {
        if (item == null)
            return;

        if (item.isCompleted)
            return;


        // Find World Space RectTransform under pointer.
        DragDropItem hitItem =
            FindWorldDropTarget(eventData);


        // =====================================================
        // CORRECT
        // =====================================================

        if (hitItem != null &&
            hitItem.matchID == item.matchID)
        {
            if (debugLogs)
            {
                Debug.Log(
                    "[DragDropManager] CORRECT DROP: " +
                    item.matchID
                );
            }

            OnCorrectMatch(item);
        }


        // =====================================================
        // WRONG
        // =====================================================

        else
        {
            if (debugLogs)
            {
                if (hitItem != null)
                {
                    Debug.Log(
                        "[DragDropManager] WRONG DROP: " +
                        item.matchID +
                        " -> " +
                        hitItem.matchID
                    );
                }
                else
                {
                    Debug.Log(
                        "[DragDropManager] DROP MISSED: " +
                        item.matchID
                    );
                }
            }

            ReturnToOrigin(item);
        }
    }


    // =========================================================
    // FIND WORLD SPACE DROP TARGET
    // =========================================================

    private DragDropItem FindWorldDropTarget(
        PointerEventData eventData
    )
    {
        if (worldSpaceCanvas == null)
        {
            Debug.LogWarning(
                "[DragDropManager] World Space Canvas is not assigned."
            );

            return null;
        }


        // -----------------------------------------------------
        // Convert screen pointer to World Space Canvas point
        // -----------------------------------------------------

        Camera canvasCamera =
            worldSpaceCanvas.worldCamera;


        if (canvasCamera == null)
        {
            canvasCamera = Camera.main;
        }


        Vector2 localPoint;


        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                worldSpaceCanvas.transform as RectTransform,
                eventData.position,
                canvasCamera,
                out localPoint))
        {
            return null;
        }


        // -----------------------------------------------------
        // Check every RectTransform
        // -----------------------------------------------------

        foreach (DragDropItem item in dragDropItems)
        {
            if (item == null)
                continue;

            if (item.worldDropTarget == null)
                continue;

            if (!item.worldDropTarget.gameObject.activeInHierarchy)
                continue;


            RectTransform target =
                item.worldDropTarget;


            // -------------------------------------------------
            // Convert world-space target to screen position
            // -------------------------------------------------

            Vector2 targetScreenPosition =
                RectTransformUtility.WorldToScreenPoint(
                    canvasCamera,
                    target.position
                );


            // -------------------------------------------------
            // Check if pointer is inside target RectTransform
            // -------------------------------------------------

            if (RectTransformUtility.RectangleContainsScreenPoint(
                    target,
                    eventData.position,
                    canvasCamera))
            {
                return item;
            }
        }


        return null;
    }


    // =========================================================
    // CORRECT MATCH
    // =========================================================

    private void OnCorrectMatch(DragDropItem item)
    {
        if (item == null)
            return;

        item.isCompleted = true;

        if (item.draggableImage != null)
        {
            item.draggableImage.gameObject.SetActive(false);
        }

        if (item.worldText != null)
        {
            item.worldText.SetActive(true);
        }

        if (debugLogs)
        {
            Debug.Log("[DragDropManager] MATCH COMPLETED: " + item.matchID);
        }

        // NEW — check if everything is done
        CheckAllComplete();
    }

    // NEW method
    private void CheckAllComplete()
    {
        if (GetCompletedCount() == dragDropItems.Count)
        {
            if (debugLogs)
            {
                Debug.Log("[DragDropManager] ALL DRAG COMPLETE — Invoking event");
            }

            onAllDragComplet?.Invoke();
        }
    }


    // =========================================================
    // RETURN TO ORIGINAL POSITION
    // =========================================================

    private void ReturnToOrigin(DragDropItem item)
    {
        if (item == null)
            return;

        if (item.draggableImage == null)
            return;


        RectTransform rect =
            item.draggableImage;


        // Restore parent
        rect.SetParent(
            item.originalParent,
            false
        );


        // Restore sibling index
        rect.SetSiblingIndex(
            item.originalSiblingIndex
        );


        // Restore position
        rect.position =
            item.originalPosition;


        // Restore rotation
        rect.rotation =
            item.originalRotation;


        // Restore scale
        rect.localScale =
            item.originalScale;


        // Make visible
        rect.gameObject.SetActive(true);


        CanvasGroup group =
            rect.GetComponent<CanvasGroup>();

        if (group != null)
        {
            group.blocksRaycasts = true;
        }
    }


    // =========================================================
    // RESET ALL
    // =========================================================

    public void ResetAllItems()
    {
        foreach (DragDropItem item in dragDropItems)
        {
            if (item == null)
                continue;


            item.isCompleted = false;


            if (item.draggableImage != null)
            {
                item.draggableImage.gameObject.SetActive(true);

                ReturnToOrigin(item);
            }


            if (item.worldText != null)
            {
                item.worldText.SetActive(false);
            }
        }
    }


    // =========================================================
    // GET COMPLETED COUNT
    // =========================================================

    public int GetCompletedCount()
    {
        int count = 0;


        foreach (DragDropItem item in dragDropItems)
        {
            if (item != null &&
                item.isCompleted)
            {
                count++;
            }
        }


        return count;
    }


    // =========================================================
    // CHECK ITEM
    // =========================================================

    public bool IsItemCompleted(string matchID)
    {
        foreach (DragDropItem item in dragDropItems)
        {
            if (item == null)
                continue;


            if (item.matchID == matchID)
            {
                return item.isCompleted;
            }
        }


        return false;
    }


    // =========================================================
    // INTERNAL DRAG HANDLER
    // =========================================================

    private class DragHandler :
        MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        private DragDropManager manager;

        private DragDropItem item;

        private Canvas screenCanvas;

        private CanvasGroup canvasGroup;

        private RectTransform rect;

        private bool keepOffset;

        private bool bringToFront;

        private Vector3 pointerOffset;

        private Camera dragCamera;


        // =====================================================
        // INITIALIZE
        // =====================================================

        public void Initialize(
            DragDropManager manager,
            DragDropItem item,
            Canvas screenCanvas,
            CanvasGroup canvasGroup,
            bool keepOffset,
            bool bringToFront
        )
        {
            this.manager = manager;

            this.item = item;

            this.screenCanvas = screenCanvas;

            this.canvasGroup = canvasGroup;

            this.keepOffset = keepOffset;

            this.bringToFront = bringToFront;

            this.rect =
                item.draggableImage;
        }


        // =====================================================
        // BEGIN DRAG
        // =====================================================

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (item == null)
                return;

            if (item.isCompleted)
                return;

            if (rect == null)
                return;


            // Disable raycast blocking while dragging.
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
            }


            // Bring to front.
            if (bringToFront)
            {
                rect.SetAsLastSibling();
            }


            // Get Screen Space Canvas camera.
            dragCamera =
                GetCanvasCamera();


            // Calculate pointer offset.
            if (keepOffset)
            {
                RectTransform parent =
                    rect.parent as RectTransform;


                if (parent != null)
                {
                    if (RectTransformUtility
                        .ScreenPointToWorldPointInRectangle(
                            parent,
                            eventData.position,
                            dragCamera,
                            out Vector3 worldPoint))
                    {
                        pointerOffset =
                            rect.position -
                            worldPoint;
                    }
                }
            }
            else
            {
                pointerOffset =
                    Vector3.zero;
            }
        }


        // =====================================================
        // DRAG
        // =====================================================

        public void OnDrag(PointerEventData eventData)
        {
            if (item == null)
                return;

            if (item.isCompleted)
                return;

            if (rect == null)
                return;

            if (screenCanvas == null)
                return;


            RectTransform parent =
                rect.parent as RectTransform;


            if (parent == null)
                return;


            // -------------------------------------------------
            // Screen Point -> Screen Canvas World Point
            // -------------------------------------------------

            if (RectTransformUtility
                .ScreenPointToWorldPointInRectangle(
                    parent,
                    eventData.position,
                    dragCamera,
                    out Vector3 worldPoint))
            {
                rect.position =
                    worldPoint +
                    pointerOffset;
            }
        }


        // =====================================================
        // END DRAG
        // =====================================================

        public void OnEndDrag(PointerEventData eventData )
        {
            if (item == null)
                return;

            if (item.isCompleted)
                return;


            // Enable raycast blocking again.
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
            }


            // Let manager handle drop.
            if (manager != null)
            {
                manager.HandleDrop(
                    item,
                    eventData
                );
            }
        }


        // =====================================================
        // GET SCREEN CANVAS CAMERA
        // =====================================================

        private Camera GetCanvasCamera()
        {
            if (screenCanvas == null)
            {
                return Camera.main;
            }


            // Screen Space Overlay
            if (screenCanvas.renderMode ==
                RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }


            // Screen Space Camera
            if (screenCanvas.worldCamera != null)
            {
                return screenCanvas.worldCamera;
            }


            return Camera.main;
        }
    }
}