using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class ThrowObject : MonoBehaviour
{
    private const int FrameCount = 7;

    private XRGrabInteractable xRGrabInteractable;

    [SerializeField] private float throwForce = 5000;
    [SerializeField] private float minimumMagnitudeToThrow;
    private List<Vector3> framePositions = new();

    private Rigidbody rigidbody;
    private Transform objectT;
    private Transform originalParent;

    private bool isInHand;

    private void Awake()
    {
        xRGrabInteractable = GetComponent<XRGrabInteractable>();
        rigidbody = GetComponent<Rigidbody>();
        objectT = transform;
        originalParent = objectT.parent;
    }

    private void OnEnable()
    {
        xRGrabInteractable.selectEntered.AddListener(OnPicked);
        xRGrabInteractable.selectExited.AddListener(OnThrow);
    }

    private void OnDisable()
    {
        xRGrabInteractable.selectEntered.RemoveAllListeners();
        xRGrabInteractable.selectExited.RemoveAllListeners();
    }

    private void OnPicked(SelectEnterEventArgs args)
    {
        framePositions.Clear();
        isInHand = true;
    }

    private void OnThrow(SelectExitEventArgs args)
    {
        // Calculate all directions
        var directions = new List<Vector3>();
        for (var i = FrameCount - 1; i > 0; i--)
        {
            var direction = framePositions[i] - framePositions[i - 1];
            directions.Add(direction);
        }

        // Find middle direction
        var middleDirection = new Vector3();
        foreach (var direction in directions)
        {
            middleDirection += direction;
        }
        middleDirection /= directions.Count;

        // Throw game object
        var throwVector = middleDirection * throwForce;
        if (throwVector.magnitude > minimumMagnitudeToThrow)
        {
            objectT.parent = null;
            rigidbody.AddForce(throwVector);
        }
        else
        {
            objectT.parent = originalParent;
        }

        isInHand = false;
    }

    private void FixedUpdate()
    {
        if (!isInHand)
        {
            return;
        }

        var positionComparedToTheParent = objectT.position - originalParent.position;
        framePositions.Add(positionComparedToTheParent);
        if (framePositions.Count > FrameCount)
        {
            framePositions.RemoveAt(0);
        }
    }
}
