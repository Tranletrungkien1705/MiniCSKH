const base = '/api/v1'
async function req(path, opts = {}) {
  const res = await fetch(base + path, {
    headers: { 'Content-Type': 'application/json' }, credentials: 'same-origin',
    ...opts, body: opts.body ? JSON.stringify(opts.body) : undefined
  })
  const text = await res.text(); const data = text ? JSON.parse(text) : null
  if (!res.ok) throw new Error(data?.error || `Lỗi ${res.status}`)
  return { data, cache: res.headers.get('X-Cache') }
}
export const api = {
  dashboard: () => req('/dashboard'),
  agents: () => req('/agents'),
  categories: () => req('/categories'),
  tickets: (status, agentId, priority, q) => req(`/tickets?${status != null ? `status=${status}&` : ''}${agentId ? `agentId=${agentId}&` : ''}${priority != null ? `priority=${priority}&` : ''}${q ? `q=${encodeURIComponent(q)}` : ''}`),
  ticket: (id) => req(`/tickets/${id}`),
  create: (b) => req('/tickets', { method: 'POST', body: b }),
  setStatus: (id, status) => req(`/tickets/${id}/status`, { method: 'POST', body: { status } }),
  setPriority: (id, priority) => req(`/tickets/${id}/priority`, { method: 'POST', body: { priority } }),
  assign: (id, agentId) => req(`/tickets/${id}/assign`, { method: 'POST', body: { agentId } }),
  comment: (id, b) => req(`/tickets/${id}/comments`, { method: 'POST', body: b }),
  kb: (q) => req(`/kb${q ? `?q=${encodeURIComponent(q)}` : ''}`),
  kbArticle: (id) => req(`/kb/${id}`),
  calls: () => req('/calls'),
  callStats: () => req('/call-stats'),
  logCall: (b) => req('/calls', { method: 'POST', body: b })
}
export const fmtDateTime = (s) => s ? new Date(s).toLocaleString('vi-VN') : '—'
export const STATUS = ['Mới', 'Đang xử lý', 'Chờ khách', 'Đã giải quyết', 'Đã đóng', 'Đã hủy']
export const STATUS_CSS = ['info', 'primary', 'warning', 'success', 'dark', 'secondary']
export const PRIORITY = ['Thấp', 'Bình thường', 'Cao', 'Khẩn cấp']
export const PRIORITY_CSS = ['secondary', 'info', 'warning', 'danger']
export const CHANNELS = ['Web', 'Email', 'Zalo', 'Điện thoại', 'Facebook']
