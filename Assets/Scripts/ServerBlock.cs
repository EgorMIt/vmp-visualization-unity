using UnityEngine;

public class ServerBlock : MonoBehaviour
{
    public GameObject model;

    private Vector3 _startPos;
    private Vector3 _targetPos;

    private bool _isMoving = false;

    private float _time;
    private float _moveSpeed = 1;

    private void Update()
    {
        if (!_isMoving) return;

        if (_time >= 1) _isMoving = false;
        _time += Time.deltaTime / _moveSpeed;
        transform.position = Vector3.Lerp(_startPos, _targetPos, _time);
    }

    public void Init(int blocks)
    {
        var tmpPos = transform.position;
        tmpPos.y += 0.15f;
        for (var block = 0; block < blocks; block++)
        {
            Instantiate(model, tmpPos, transform.rotation, transform);
            tmpPos.y += 0.3f;
        }
    }

    public void MoveForward(float amount)
    {
        _startPos = transform.position;
        _targetPos = _startPos;
        _targetPos.x += amount;
        _time = 0;
        _isMoving = true;
    }
}