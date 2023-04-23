using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIControl : MonoBehaviour
{
    public GameObject gameCamera;
    public Algorithms main;
    public float speed = 1.0f;
    public float maxX = 100;
    public float minX = -100;

    private bool _isDragging, _isOver = true;

    public bool canControl = false;

    private VisualElement _root;

    private Button _clear, _next, _sort;
    public DropdownField algorithms;

    private Slider _mutation, _temperature, _cooling;
    private SliderInt _iteration;
    private Label _parameter1, _parameter2;
    private ProgressBar _progressBar;

    private VisualElement _genetic, _annealing;

    private void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _clear = _root.Q<Button>("clear");
        _next = _root.Q<Button>("next");
        _sort = _root.Q<Button>("sort");
        
        algorithms = _root.Q<DropdownField>("algoritm");
        _clear.clicked += () => main.Reset();
        _next.clicked += () => Next();
        _sort.clicked += () => Sort();
        
        _mutation = _root.Q<Slider>("mutation");
        _temperature = _root.Q<Slider>("temperature");
        _cooling = _root.Q<Slider>("cooling");
        _iteration = _root.Q<SliderInt>("iter");

        _parameter1 = _root.Q<Label>("iteration");
        _parameter2 = _root.Q<Label>("parameter");

        _root.Q<Button>("reload").clicked += () => ReloadScene();

        _progressBar = _root.Q<ProgressBar>("progress");
        _genetic = _root.Q<VisualElement>("geneticParameters");
        _annealing = _root.Q<VisualElement>("annealingParameters");

        _mutation.RegisterValueChangedCallback(v =>
        {
            _mutation.label = "Mutation - " + v.newValue;
            main.mutationRate = v.newValue;
        });
        _temperature.RegisterValueChangedCallback(v =>
        {
            _temperature.label = "Temperature - " + v.newValue;
            main.startTemperature = v.newValue;
        });
        _cooling.RegisterValueChangedCallback(v =>
        {
            _cooling.label = "Cooling - " + v.newValue;
            main.coolingRate = v.newValue;
        });

        _iteration.RegisterValueChangedCallback(v =>
        {
            _iteration.label = "Iterations - " + v.newValue;
            main.maxIterations = v.newValue;
        });

        algorithms.RegisterValueChangedCallback(v =>
        {
            switch (algorithms.value)
            {
                case "Simulated annealing":
                    _genetic.visible = false;
                    _annealing.visible = true;
                    _parameter2.visible = true;
                    break;
                case "Genetic":
                    _genetic.visible = true;
                    _annealing.visible = false;
                    _parameter2.visible = true;
                    break;
                default:
                    _genetic.visible = false;
                    _annealing.visible = false;
                    _parameter2.visible = false;
                    break;
            }
        });
        _genetic.visible = false;
        _annealing.visible = false;
        _parameter2.visible = false;

        _annealing.RegisterCallback<MouseEnterEvent>(v => { _isOver = false; });
        _annealing.RegisterCallback<MouseLeaveEvent>(v => { _isOver = true; });
        _genetic.RegisterCallback<MouseEnterEvent>(v => { _isOver = false; });
        _genetic.RegisterCallback<MouseLeaveEvent>(v => { _isOver = true; });

        _root.visible = canControl;
    }

    private void Update()
    {
        _root.visible = canControl;

        switch (algorithms.value)
        {
            case "Simulated annealing":
                _parameter1.text = "Current temperature \n" + main.temperature + " / " + main.startTemperature;
                _parameter2.text = "Effectiveness - " + main.currentEnergy;
                _progressBar.lowValue = 1.0f - main.temperature / main.startTemperature;
                break;
            case "Genetic":
                _parameter1.text = "Current iteration \n" + main.currentIteration + " / " + main.maxIterations;
                _parameter2.text = "Population - " + main.population;
                _progressBar.lowValue = (float)main.currentIteration / main.maxIterations;
                break;
            default:
                _parameter1.text = "Current server - \n" + main.currentServer + " / " + main.servers.Length;
                _progressBar.lowValue = (float)main.currentServer / main.servers.Length;
                break;
        }

        if (Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
        }

        if (_isDragging && _isOver)
        {
            var horizontal = Input.GetAxis("Mouse X") * -speed * Time.deltaTime;
            var newPos = new Vector3
            {
                x = horizontal
            };
            if (gameCamera.transform.position.x + newPos.x > maxX)
                newPos.x = 0;
            if (gameCamera.transform.position.x + newPos.x < minX)
                newPos.x = 0;
            gameCamera.transform.position += newPos;
        }
    }

    private void Next()
    {
        switch (algorithms.value)
        {
            case "First fit":
                StartCoroutine(main.SortFirstFitOne());
                break;
            case "Best fit":
                StartCoroutine(main.SortBestFitOne());
                break;
            case "Simulated annealing":
                StartCoroutine(main.SortSimAnnealingOne());
                break;
            case "Genetic":
                StartCoroutine(main.SortGeneticOne());
                break;
        }

        main.audioPlayer.PlayOneShot(main.audioClip1);
    }

    private void Sort()
    {
        switch (algorithms.value)
        {
            case "First fit":
                StartCoroutine(main.SortFirstFit());
                break;
            case "Best fit":
                StartCoroutine(main.SortBestFit());
                break;
            case "Simulated annealing":
                StartCoroutine(main.SortSimAnnealing());
                break;
            case "Genetic":
                StartCoroutine(main.SortGenetic());
                break;
        }

        main.audioPlayer.PlayOneShot(main.audioClip2);
    }


    public void DisableUI()
    {
        _clear.SetEnabled(false);
        _next.SetEnabled(false);
        _sort.SetEnabled(false);
        algorithms.SetEnabled(false);
    }

    public void EnableUI()
    {
        _clear.SetEnabled(true);
        _next.SetEnabled(true);
        _sort.SetEnabled(true);
        //algoritms.SetEnabled(true);
    }

    private static void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}