# Gmail OAuth Refresh Token 重新授权指引

## 背景

`GmailService` 使用 OAuth2 `refresh_token` 换取 access token 后调用 Gmail API 发信。当前 `refresh_token` 已失效，Google 返回 `400 invalid_grant`。

常见失效原因（任一命中都会导致 `invalid_grant`）：

- 发件 Gmail 账号修改了密码（Google 对包含 Gmail 等敏感 scope 的授权，密码变更后会主动作废旧 refresh token）
- 用户在 Google 账号的"第三方应用授权"里手动撤销了访问权限
- refresh token 超过 6 个月未使用
- OAuth 客户端处于"测试"(Testing) 发布状态，refresh token 签发后 7 天自动过期
- client_secret 被检测为泄露（如提交进公开 git 仓库），Google 自动吊销相关凭据

## 前置准备

- 用**发件 Gmail 账号**本人登录（就是收发邮件用的那个账号）
- 现有的 `GmailClientId` / `GmailClientSecret`（在 `appsettings.json` 里）
- Google Cloud Console 的访问权限（用来检查/修改 OAuth 客户端配置）

## 步骤一：检查 OAuth 客户端配置

1. 打开 [Google Cloud Console](https://console.cloud.google.com/) → APIs & Services → Credentials
2. 找到对应的 OAuth 2.0 Client ID（即 `GmailClientId` 那一串 `xxxx.apps.googleusercontent.com`）
3. 点击编辑，检查"已获授权的重定向 URI"列表，确认包含：
   ```
   https://developers.google.com/oauthplayground
   ```
   没有的话先添加并保存
4. 检查 OAuth 同意屏幕 (OAuth consent screen) 的**发布状态**：
   - 如果是 **Testing（测试）**：新生成的 refresh token 只有 7 天有效期，7 天后会重复出现今天这个问题。需要把状态改为 **In production（生产）**，或者如果只给内部账号用，也可以把"User type"设为 **Internal**，同样不受 7 天限制。
   - 如果已经是 **In production**：跳过此项

## 步骤二：用 OAuth Playground 生成新的 refresh_token

1. 打开 https://developers.google.com/oauthplayground

2. 点右上角齿轮图标 (OAuth 2.0 Configuration)，勾选 **"Use your own OAuth credentials"**，填入：
   - OAuth Client ID: `GmailClientId` 的值
   - OAuth Client secret: `GmailClientSecret` 的值

3. 左侧 **Step 1 - Select & authorize APIs**，在输入框中手动填入 scope：
   ```
   https://www.googleapis.com/auth/gmail.send
   ```
   （只需要发信权限，跟代码里 `SendEmailAsync` 调用的接口对应，不要多授权用不到的 scope）

4. 点击 **Authorize APIs**，浏览器会跳转到 Google 登录页 —— 用**发件人 Gmail 账号**登录并同意授权

5. 授权完成后自动跳回 Playground，**Step 2 - Exchange authorization code for tokens**，点击 **Exchange authorization code for tokens**

6. 在返回的响应里找到 `refresh_token` 字段，完整复制下来（这一步生成的 refresh token 只会显示一次，务必立即保存）

## 步骤三：更新配置并重启服务

1. 打开 `appsettings.json`，更新：
   ```json
   "GmailRefreshToken": "<步骤二里拿到的新 refresh_token>"
   ```
2. 保存后重新部署/重启服务，使新配置生效

## 步骤四：验证

1. 触发一次实际的发信流程（例如提交一次测试 webhook，或直接调用发信接口）
2. 查看服务日志，确认：
   - `POST https://oauth2.googleapis.com/token` 返回 `200` 而不是 `400`
   - 后续 `邮件发送成功到: ...` 日志出现
3. 如果仍然报 `400 invalid_grant`，重新检查：
   - Client ID / Secret 是否跟 Playground 里用的完全一致
   - 步骤一里的发布状态是否确实是 In production / Internal
   - 是否用对了发件账号登录

## 安全提醒

`GmailClientSecret` 和 `GmailRefreshToken` 目前以明文形式保存在 `appsettings.json` 并已提交进 git 历史。这次授权失效很可能与此有关（泄露的凭据被 Google 自动吊销）。建议后续将这几项配置迁移到环境变量或部署平台的 secret 管理中，避免同样的问题重复出现。
