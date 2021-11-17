using System.Collections.Generic;
using System.Net;

namespace Cpdaily.LoginWorkers
{
    public class LoginParameter
    {
        /// <summary>
        ///  学号
        /// </summary>
        public string? Username { get; set; } = null;

        /// <summary>
        /// 明文密码
        /// </summary>
        public string? Password { get; set; } = null;

        /// <summary>
        /// 加密后密码
        /// </summary>
        public string? EncryptedPassword { get; set; } = null;

        /// <summary>
        /// Ids Url
        /// </summary>
        public string? IdsUrl { get; set; } = null;

        /// <summary>
        /// 实际登录的网址
        /// </summary>
        public string? ActionUrl { get; set; } = null;

        /// <summary>
        /// 是否需要验证码，如果为 true 则需要验证码。
        /// 通过 CaptchaImageUrl 和 CookieContainer 属性获取验证码，
        /// 并将验证码结果填写到 CaptchaValue 中。
        /// </summary>
        public bool NeedCaptcha { get; set; } = false;

        /// <summary>
        /// 验证码地址
        /// </summary>
        public string? CaptchaImageUrl { get; set; } = null;

        /// <summary>
        /// 验证码结果
        /// </summary>
        public string? CaptchaValue { get; set; } = null;

        /// <summary>
        /// 用于登录的一些参数
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 一般用于获取验证码。登录成功后的 Cookie 也可在这里获取。
        /// </summary>
        public CookieContainer CookieContainer { get; set; } = new CookieContainer();
    }
}
