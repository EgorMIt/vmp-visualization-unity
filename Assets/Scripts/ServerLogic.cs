using System.Collections;
using UnityEngine;

public class ServerLogic : MonoBehaviour
{
    public Transform currentServer;
    public float moveTime;
    public float moveSpeed = 1;

    private Vector3 _startPos;
    private Vector3 _targetPos;
    private bool _isMoving = false;
    private float _time;

    private void Update()
    {
        if (!_isMoving) return;

        _time += Time.deltaTime / moveTime;
        _startPos = currentServer.transform.position;
        currentServer.position = Vector3.Lerp(_startPos, _targetPos, moveSpeed * Time.deltaTime);
    }

    public void MoveServerTo(Transform server, Vector3 target, float t)
    {
        _time = 0;
        currentServer = server;
        _startPos = server.position;
        _targetPos = target;
        moveTime = t;
        _isMoving = true;
        StartCoroutine(Wait(t));
        //yield return new WaitForSeconds(moveTime);
    }

    private IEnumerator Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _isMoving = false;
    }
}