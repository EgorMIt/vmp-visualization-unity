using UnityEngine;
using UnityEngine.UIElements;

public class StartScreen : MonoBehaviour
{
    public ConfigServers config;

    private VisualElement _root;
    private SliderInt _stands, _servers;
    private UIDocument _configUi;

    private void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _stands = _root.Q<SliderInt>("stands");
        _servers = _root.Q<SliderInt>("servers");
        _root.Q<Button>("confirm").clicked += () => Confirm();
    }

    private void Update()
    {
        _stands.label = "Stands - " + _stands.value;
        _servers.label = "Servers - " + _servers.value;
    }

    private void Confirm()
    {
        _root.visible = false;
        config.Init(_stands.value, _servers.value);
    }
}