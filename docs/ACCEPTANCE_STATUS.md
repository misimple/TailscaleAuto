# Acceptance status / 验收状态

## English

| Evidence | Recorded status |
| --- | --- |
| Real Windows use | The user reports successful Tailscale RDP and LAN RDP connections on multiple real Windows devices. No device count, test date, or exact build number was supplied with this feedback. |
| Automated checks | Release-preparation automated tests and simulated LAN integration checks were reported as passed. This documentation correction did not rerun them. |
| Version-specific live acceptance | No separate live test record identifying the `1.6.14.0-dev` build is available. The user report above is not labeled as a version-specific test. |
| Screenshot | The published screenshots use synthetic device data and show the real WinForms UI; they are not evidence of a live RDP connection. |

LAN RDP requires a reachable physical LAN route, Remote Desktop enabled on the target, and reachable TCP 3389. Tailscale RDP requires a working Tailscale connection and Remote Desktop enabled on the target.

## 简体中文

| 证据来源 | 记录状态 |
| --- | --- |
| 真实 Windows 使用 | 用户反馈 Tailscale RDP 和局域网 RDP 均已在多台真实 Windows 设备上成功连接。此次反馈未附具体设备数量、测试日期或准确构建号。 |
| 自动化检查 | 发布准备阶段的自动测试与模拟 LAN 集成检查记录为通过；本次仅修正文档，没有重新运行测试。 |
| 特定版本真实验收 | 尚无单独留档、能明确对应 `1.6.14.0-dev` 构建的真实环境测试记录。上述用户反馈不标记为该版本的独立测试。 |
| 截图 | 已发布截图使用合成设备数据，展示真实 WinForms 界面，不作为真实 RDP 连接证据。 |

局域网 RDP 需要物理局域网路由可达、目标设备启用远程桌面服务、TCP 3389 可达。Tailscale RDP 需要 Tailscale 连接可用，且目标设备启用远程桌面服务。
