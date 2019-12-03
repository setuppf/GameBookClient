using System;

namespace GEngine
{
    [Flags]
    public enum UiType : int
    {
        [EditorEnum("无")]
        None = 0,

        [EditorEnum("登录界面")]
        Login = 1,

        [EditorEnum("选择角色界面")]
        Roles = 2,

        RoleCreate = 3,
        RoleSelect = 4,

        LoadingBar = 100,
        ModalBox0, // （模态）没有Btn的一个提示窗口，事件触发，事件关闭
        ModalBox1, // （模态）有一个确认Btn
        ModalBox2, // （模态）有一个确认Btn，一个取消Btn
    }
}

