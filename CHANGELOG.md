# Changelog

## 1.6.14.0-dev — public review candidate

- Integrated the previously verified LAN RDP core into the current bilingual WinForms application. LAN availability requires a matching Tailscale direct endpoint, a physical on-link route, and TCP 3389. Launch rechecks the result.
- Preserved Tailscale RDP as a separate action using the Tailscale IP.
- Included current UI layout and button-border fixes, bilingual interface, floating monitor, tray behavior, and embedded original generic device icons.
- LAN integration passed automated simulated checks. This candidate has not yet passed a new live two-device LAN acceptance test.
