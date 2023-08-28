# Prometheus Setup
## Adding The Prometheus Entry

> Change 5108 to the port that you have defined

```yaml
- job_name: 'wakabot'
  static_configs:
    - targets: ['localhost:5108']
```

## Generated Metrics

| Metric | Explanation |
|---|---|
| waka_uptime | Bot uptime in seconds |
| waka_memory_usage | Server memory usage |
| waka_users | Total amount of registered users |
| waka_guilds | Total amount of guilds |
| waka_cache_hits | Total amount of cache hits |
| waka_cache_misses | Total amount of cache misses |

For the "total" metrics you might want to use [https://prometheus.io/docs/prometheus/latest/querying/functions/#irate](irate).
