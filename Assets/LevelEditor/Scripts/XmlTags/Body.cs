public class Body : GameObjectTag
{
    public bool dynamic = false;

    private void Start()
    {
        gameObject.name = "<body>";
    }
}
