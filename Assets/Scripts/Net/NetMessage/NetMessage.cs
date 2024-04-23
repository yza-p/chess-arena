using Unity.Collections;
using UnityEngine;

public enum OpCode
{
    KEEP_ALIVE = 0,
    WELCOME = 1,
    START_GAME = 2,
    MAKE_MOVE = 4,
    REMATCH = 5
}

public class NetMessage : MonoBehaviour
{
    public OpCode Code { get; set; }

    public virtual void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
    }
    public virtual void Deserialize(ref DataStreamReader reader)
    {

    }

    public virtual void ReceivedOnClient()
    {

    }
    public virtual void ReceivedOnServer()
    {

    }

}
